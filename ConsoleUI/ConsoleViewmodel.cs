using MarketDataProvider;
using MarketDataProvider.Exceptions;
using MarketDataProvider.Factories;

namespace ConsoleUI;

internal class ConsoleViewmodel
{
    public event Action<string>? NewNotification;
    public event Action<string>? ConnectionChanged;

    public event Action<string, Action<string>>? UserPromptRequested;
    public event Action<string>? NewContent;
    public event Action? ContentKindChanged;
    public event Action? SubscriptionChanged;

    public string CommandsDescriptionContent =
        $$"""
                    Press Ctrl+K to connect to the server
                    Press Ctrl+D to disconnect from the server

                    Press Ctrl+S to subscribe to trades
                    Press Ctrl+U to unsubscribe from trades

                    Press Ctrl+E to output available securities
                    Press Ctrl+T to output subscribed trades

                    Press Ctrl+H to rewind to the top of the console
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
        get => _content; 
        private set
        {
            _content = value;
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

        CommandsManager.ShowTradesCommandRequested += OnShowTradesCommandRequested;
        CommandsManager.ShowSecuritiesCommandRequested += OnShowSecuritiesCommandRequested;

        _connection = ConnectionsFactory.CreateByBitConnection();
        _connection.ConnectionStateChanged += OnConnectionStateChanged;
        _marketDataProvider = MarketDataProvidersFactory.CreateBybitProvider(_connection as IConnectableDataTransmitter);
        _marketDataProvider.NewTrades += OnNewTrades;
    }


    private bool TryFindSecurity(string? value, out ISecurity? security)
    {
        if (string.IsNullOrEmpty(value))
        {
            security = default;
            return false;
        }

        return _availableSecurities.TryGetValue(value, out security);
    }
    private void ProcessNewSecurities(IEnumerable<ISecurity> securities)
    {
        _availableSecurities.Clear();

        foreach (var security in securities)
        {
            _availableSecurities[security.Ticker] = security;
        }
    }

    private void OnNewTrades(object? sender, ITrade e)
    {
        _trades.Add(e);

        if (_outputKind == OutputKind.Trades)
        {
            NewContent?.Invoke(e.ToString());
        }
    }
    private void OnConnectionStateChanged(object? sender, ConnectionState state)
    {
        if (state == MarketDataProvider.ConnectionState.Disconnected)
        {
            _subscriptions.Clear();
            SubscriptionChanged?.Invoke();
        }
        ConnectionChanged?.Invoke(state.ToString());
    }

    private void OnUnsubscribeCommandRequested()
    {
        if (!_subscriptions.Any())
        {
            NewNotification?.Invoke("Not subscribed to anything");
            return;
        }

        var message = "Security to unsubscribe: ";
        UserPromptRequested?.Invoke(message, OnUserRequestedUnsubscription);
    }
    private void OnUserRequestedUnsubscription(string? ticker)
    {
        if (!_subscriptions.Contains(ticker))
        {
            NewNotification?.Invoke($"Not subscribed to '{ticker}'");
            return;
        }

        if (!TryFindSecurity(ticker, out var security))
        {
            NewNotification?.Invoke($"Security '{ticker}' not found");
            return;
        }

        NewNotification?.Invoke($"Cancelling subscribtion to trades for '{ticker}'..");

        try
        {
            _marketDataProvider.UnsubscribeTradesAsync(security!).Wait();
            _subscriptions.Remove(ticker!);
            NewNotification?.Invoke(string.Empty);
            SubscriptionChanged?.Invoke();
        }
        catch (Exception e)
        {
            NewNotification?.Invoke(e.Message);
        }
    }

    private void OnSubscribeCommandRequested()
    {
        if (!_availableSecurities.Any())
        {
            NewNotification?.Invoke($"Must request the list of securities from the server first");
            return;
        }

        if (_connection.ConnectionState != MarketDataProvider.ConnectionState.Connected)
        {
            NewNotification?.Invoke("Cannot request subscription when disconnected");
            return;
        }

        var message = "Security to subscribe: ";
        UserPromptRequested?.Invoke(message, OnUserRequestedTrades);
    }
    private void OnUserRequestedTrades(string? ticker)
    {
        if (_subscriptions.Contains(ticker))
        {
            NewNotification?.Invoke($"Already subscribed to '{ticker}'");
            return;
        }

        if (!TryFindSecurity(ticker, out var security))
        {
            NewNotification?.Invoke($"Security '{ticker}' not found");
            return;
        }

        NewNotification?.Invoke($"Requesting trades for '{ticker}'..");

        try
        {
            _marketDataProvider.SubscribeTradesAsync(security!).Wait();
            _subscriptions.Add(ticker!);
            NewNotification?.Invoke(string.Empty);
            SubscriptionChanged?.Invoke();
        }
        catch (Exception e)
        {
            NewNotification?.Invoke(e.Message);
        }
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

                NewNotification?.Invoke("Requesting securities..");
                var securities = _marketDataProvider.GetAvailablSecuritiesAsync(filter).Result;

                if (securities.Any())
                {
                    NewNotification?.Invoke("Securities successfully received");
                    ProcessNewSecurities(securities);
                    SetContent(OutputKind.Securities);
                    return;
                }
            }
            catch { }

            NewNotification?.Invoke("Could not get securities from server..");
        }

        SetContent(OutputKind.Securities);
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
            ConfigurationManager.DeleteConfiguration();

            NewNotification?.Invoke($"Configuration parameter {e.Message} was invalid. Gonna try with default one");
            var newparams = ConfigurationManager.CreateDefaultParameters();

            try
            {
                _connection.ConnectAsync(newparams, CancellationToken.None).Wait();
                NewNotification?.Invoke("Successfully connected with new configuration");
                ConfigurationManager.SaveConnectionParametersAsync(newparams);
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
            OutputKind.Securities => _availableSecurities.Values.Select(s => s.ToString()).ToArray(),
            OutputKind.Trades => _trades.Select(t => t.ToString()).ToArray(),
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
    private readonly IConnection _connection;
    private readonly IMarketDataProvider _marketDataProvider;
    private readonly HashSet<string> _subscriptions = new(StringComparer.InvariantCultureIgnoreCase);
    private readonly Dictionary<string,ISecurity> _availableSecurities = new(1000, StringComparer.InvariantCultureIgnoreCase);
    private readonly HashSet<ITrade> _trades = new(10000);
    private IEnumerable<string> _content = Enumerable.Empty<string>();
}
