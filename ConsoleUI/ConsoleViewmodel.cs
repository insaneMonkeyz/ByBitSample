using MarketDataProvider;

namespace ConsoleUI;

internal class ConsoleViewmodel
{
    public event Action<string>? NewNotification;
    public event Action<string>? ConnectionChanged;

    public event Action<Func<string>>? UserPromptRequested;
    public event Action<string>? NewContent;
    public event Action? ContentKindChanged;

    public string CommandsDescriptionContent =
        $$"""
                    Press Ctrl+C to connect to the server
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

    public IEnumerable<string> Content { get; private set; } = Enumerable.Empty<string>();

    public ConsoleViewmodel()
    {
        CommandsManager.ListenUserCommands();
        CommandsManager.DisconnectCommandRequested += OnDisconnectCommandRequested;
        CommandsManager.ConnectCommandRequested += OnConnectCommandRequested;

        CommandsManager.ShowLogCommandRequested += OnShowLogCommandRequested;
        CommandsManager.ShowTradesCommandRequested += OnShowTradesCommandRequested;
        CommandsManager.ShowSecuritiesCommandRequested += OnShowSecuritiesCommandRequested;

        //_connection.ConnectionStateChanged += (_, state) => ConnectionChanged?.Invoke(state.ToString());
    }

    private void OnShowSecuritiesCommandRequested()
    {
        Content = new[]
        {
            "BTCUSDT",
            "ETHUSDT",
            "MNRUSDT",
        };
        ContentKindChanged?.Invoke();
    }

    private void OnShowLogCommandRequested()
    {
        Content = new[]
        {
            $"{DateTime.UtcNow:O} Initialized everything",
            $"{DateTime.UtcNow:O} Connection state changed to disconnected",
            $"{DateTime.UtcNow:O} Closing",
        };
        ContentKindChanged?.Invoke();
    }

    private void OnShowTradesCommandRequested()
    {
        Content = new[]
        {
            $"{DateTime.UtcNow:O} BTCUSDT BUY  768 x 29875.1",
            $"{DateTime.UtcNow:O} ETHUSDT SELL 435 x 2075.94",
            $"{DateTime.UtcNow:O} MNRUSDT BUY    4 x 825.15",
        };
        ContentKindChanged?.Invoke();
    }

    private void OnConnectCommandRequested()
    {
        try
        {
        }
        catch (Exception)
        {

            throw;
        }
    }

    private void OnDisconnectCommandRequested()
    {
        throw new NotImplementedException();
    }

    private IConnection _connection;
    private readonly List<string> _subscriptions = new();
    private readonly List<string> _available = new(1000);
    private readonly Queue<string> _logQueue = new(10000);
    private readonly Queue<string> _tradesQueue = new(10000);
}
