using System.Xml.Serialization;
using MarketDataProvider;
using Microsoft.Extensions.Logging;

internal class Program
{
    private const string ConnectionConfigurationFile = "ConnectionConfiguration.xml";
    private static IConnection _bybit;

    private static async Task Main(string[] args)
    {
        Initialize();

        Console.WriteLine($"requesting connection to the exchange");

        var configuration = await GetOrCreateConnectionParametersAsync();

        await _bybit.ConnectAsync(configuration, CancellationToken.None);

        for (int i = 0; i < 12 && _bybit.ConnectionState != ConnectionState.Disconnected; i++)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
        }

        await _bybit.DisconnectAsync(CancellationToken.None);
    }

    private static ConnectionParameters CreateDefaultParameters()
    {
        return new()
        {
            Uri = "wss://stream.bybit.com/v5/public/spot",
            ReconnectionAttempts = 3,
            ConnectionTimeout = TimeSpan.FromSeconds(10),
            HeartbeatInterval = TimeSpan.FromSeconds(20),
            ReconnectionInterval = TimeSpan.FromSeconds(30),
            UseHeartbeating = true,
        };
    }
    private static async Task<ConnectionParameters> GetOrCreateConnectionParametersAsync()
    {
        var parameters = await LoadConnectionParametersAsync();

        if (parameters is null)
        {
            parameters = CreateDefaultParameters();
            await SaveConnectionParametersAsync(parameters);
        }

        return parameters;
    }
    private static async Task SaveConnectionParametersAsync(ConnectionParameters parameters)
    {
        await Task.Run(() =>
        {
            var serializer = new XmlSerializer(typeof(ConnectionParameters));
            var fileStream = new FileStream(ConnectionConfigurationFile, FileMode.Create, FileAccess.Write);
            using var writer = new StreamWriter(fileStream);

            serializer.Serialize(writer, parameters);
        });
    }
    private static async Task<ConnectionParameters?> LoadConnectionParametersAsync()
    {
        if (!File.Exists(ConnectionConfigurationFile))
        {
            return null;
        }

        return await Task.Run(() =>
        {
            using var fileStream = new FileStream(ConnectionConfigurationFile, FileMode.Open, FileAccess.Read);
            var serializer = new XmlSerializer(typeof(ConnectionParameters));
            return serializer.Deserialize(fileStream) as ConnectionParameters;
        });
    }

    private static void Initialize()
    {
        _bybit = ConnectionsFactory.CreateByBitConnection(null);
        _bybit.ConnectionStateChanged += (sender, state) => Console.WriteLine($"State changed: {state}");
    }
}