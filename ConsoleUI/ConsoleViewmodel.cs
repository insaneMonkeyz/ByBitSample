using MarketDataProvider;
using MarketDataProvider.Exceptions;

namespace ConsoleUI;

internal class ConsoleViewmodel
{
    public event Action<string>? NewNotification;
    public event Action<string>? ConnectionChanged;

    public event Action<string, Action<string>>? UserPromptRequested;
    public event Action<string>? NewContent;
    public event Action? ContentKindChanged;

    public string CommandsDescriptionContent =
        $$"""
                    Press Ctrl+K to connect to the server
                    Press Ctrl+D to disconnect from the server

                    Press Ctrl+S to subscribe to trades
                    Press Ctrl+U to unsubscribe from trades

                    Press Ctrl+E to output available securities
                    Press Ctrl+T to output subscribed trades
                    Press Ctrl+L to output log
            """;
    public string SubscriptionsContent
    {
        get
        {
            return _subscriptions.Count > 0
                ? string.Join(Environment.NewLine + "\t", _subscriptions)
                : "No subscriptions yet";
        }
    }

    public IEnumerable<string> Content 
    { 
        get => content; 
        private set
        {
            content = value;
            ContentKindChanged?.Invoke();
        }
    }

    public string ConnectionState
    {
        get
        {
            var state = _connection?.ConnectionState 
                ?? MarketDataProvider.ConnectionState.Disconnected;

            return state.ToString();
        }
    }

    public ConsoleViewmodel()
    {
        CommandsManager.ListenUserCommands();
        CommandsManager.DisconnectCommandRequested += OnDisconnectCommandRequested;
        CommandsManager.ConnectCommandRequested += OnConnectCommandRequested;

        CommandsManager.SubscribeCommandRequested += OnSubscribeCommandRequested;
        CommandsManager.UnsubscribeCommandRequested += OnUnsubscribeCommandRequested;

        CommandsManager.ShowLogCommandRequested += OnShowLogCommandRequested;
        CommandsManager.ShowTradesCommandRequested += OnShowTradesCommandRequested;
        CommandsManager.ShowSecuritiesCommandRequested += OnShowSecuritiesCommandRequested;

        _connection = ConnectionsFactory.CreateByBitConnection();
        _marketDataProvider = new BybitMarketDataProvider(_connection as IConnectableDataTransmitter);
        _connection.ConnectionStateChanged += (_, state) => ConnectionChanged?.Invoke(state.ToString());
    }

    private void OnUnsubscribeCommandRequested()
    {
        throw new NotImplementedException();
    }
    private void OnSubscribeCommandRequested()
    {
        if (_connection.ConnectionState != MarketDataProvider.ConnectionState.Connected)
        {
            NewNotification?.Invoke("Cannot request subscription when disconnected");
            return;
        }

        void processUserInpt(string input)
        {
        }
        var message = "Please provide security ticker to subscribe: ";
        UserPromptRequested?.Invoke(message, processUserInpt);
    }

    private void OnShowSecuritiesCommandRequested()
    {
        if (_availableSecurities.Count == 0)
        {
            var filter = new SecurityFilter
            {
                EntityType = TradingEntityType.Cryptocurrency,
                Kind = SecurityKind.Spot,
            };

            try
            {
                var task = _marketDataProvider.GetAvailablSecuritiesAsync(filter);

                NewNotification?.Invoke("Requesting securities..");

                var securities = task.Result;

                if (securities.Any())
                {
                    NewNotification?.Invoke("Securities successfully received");
                    _availableSecurities.AddRange(securities.Select(s => $"{s.Kind}\t{s.Ticker}"));
                    SetContent(OutputKind.Securities);
                    return;
                }
            }
            catch { }

            NewNotification?.Invoke("Could not get securities from server..");
        }

        SetContent(OutputKind.Securities);
    }
    private void OnShowLogCommandRequested()
    {
        SetContent(OutputKind.Log);
    }
    private void OnShowTradesCommandRequested()
    {
        SetContent(OutputKind.Trades);
    }

    private void OnConnectCommandRequested()
    {
        try
        {
            if (_connection.ConnectionState == MarketDataProvider.ConnectionState.Connected)
            {
                NewNotification?.Invoke("Already connected");
                return;
            }
            else if (_connection.ConnectionState == MarketDataProvider.ConnectionState.Connecting)
            {
                NewNotification?.Invoke("Still trying to connect");
                return;
            }

            NewNotification?.Invoke("Loading connection configuration..");
            var parameters = ConfigurationManager.GetOrCreateConnectionParametersAsync().Result;

            NewNotification?.Invoke("Connecting..");
            _connection.ConnectAsync(parameters, CancellationToken.None).Wait();
            NewNotification?.Invoke(string.Empty);
        }
        catch (AggregateException ae) when (ae.InnerException is InvalidConfigurationException e)
        {
            NewNotification?.Invoke($"Configuration parameter {e.Message} was invalid. Gonna try with default one");
            var newparams = ConfigurationManager.CreateDefaultParameters();
            ConfigurationManager.SaveConnectionParametersAsync(newparams);

            try
            {
                _connection.ConnectAsync(newparams, CancellationToken.None).Wait();
                NewNotification?.Invoke("Successfully connected with new configuration");
            }
            catch
            {
                NewNotification?.Invoke("Connection attempt failed completely");
            }

        }
        catch (AggregateException ae) when (ae.InnerException is ConnectionException e)
        {
            NewNotification?.Invoke(e.Message);
        }
        catch (Exception e)
        {
            NewNotification?.Invoke("Could not establish connection");
        }
    }
    private void OnDisconnectCommandRequested()
    {
        try
        {
            if (_connection.ConnectionState == MarketDataProvider.ConnectionState.Disconnecting ||
                _connection.ConnectionState == MarketDataProvider.ConnectionState.Disconnected)
            {
                NewNotification?.Invoke("Already disconnected");
                return;
            }

            NewNotification?.Invoke("Disconnecting..");
            _connection.DisconnectAsync(CancellationToken.None).Wait();
            NewNotification?.Invoke(string.Empty);
        }
        catch
        {
            NewNotification?.Invoke("Error while disconnecting");
        }
    }

    private void SetContent(OutputKind kind)
    {
        _outputKind = kind;
        Content = kind switch
        {
            OutputKind.Securities => _availableSecurities,
            OutputKind.Trades => _tradesQueue,
            OutputKind.Log => _logQueue,
                     _ => Enumerable.Empty<string>()
        };
    }

    private enum OutputKind
    {
        Securities,
        Trades,
        Log,
    }

    private OutputKind _outputKind;
    private IConnection _connection;
    private IMarketDataProvider _marketDataProvider;
    private IEnumerable<string> content = Enumerable.Empty<string>();
    private readonly List<string> _subscriptions = new();
    private readonly List<string> _availableSecurities = new(1000);
    private readonly Queue<string> _logQueue = new(10000);
    private readonly Queue<string> _tradesQueue = new(10000);
}
