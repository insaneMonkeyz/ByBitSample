using System.Xml.Serialization;
using MarketDataProvider;

internal static class ConfigurationManager
{
    private const string ConnectionConfigurationFile = "ConnectionConfiguration.xml";

    public static async Task<ConnectionParameters> GetOrCreateConnectionParametersAsync()
    {
        var parameters = await LoadConnectionParametersAsync();

        if (parameters is null)
        {
            parameters = CreateDefaultParameters();
            await SaveConnectionParametersAsync(parameters);
        }

        return parameters;
    }
    public static ConnectionParameters CreateDefaultParameters()
    {
        return new()
        {
            StreamHost = "wss://stream.bybit.com/v5/public/spot",
            RestHost = "api-testnet.bybit.com",
            ReconnectionAttempts = 3,
            ConnectionTimeout = TimeSpan.FromSeconds(10),
            HeartbeatInterval = TimeSpan.FromSeconds(20),
            ReconnectionInterval = TimeSpan.FromSeconds(30),
            UseHeartbeating = true,
        };
    }
    public static async Task<ConnectionParameters?> LoadConnectionParametersAsync()
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
    public static async Task SaveConnectionParametersAsync(ConnectionParameters parameters)
    {
        await Task.Run(() =>
        {
            var serializer = new XmlSerializer(typeof(ConnectionParameters));
            var fileStream = new FileStream(ConnectionConfigurationFile, FileMode.Create, FileAccess.Write);
            using var writer = new StreamWriter(fileStream);

            serializer.Serialize(writer, parameters);
        });
    }
    public static void DeleteConfiguration()
    {
        if (File.Exists(ConnectionConfigurationFile))
        {
            File.SetAttributes(ConnectionConfigurationFile, FileAttributes.Normal);
            File.Delete(ConnectionConfigurationFile);
        }
    }
}