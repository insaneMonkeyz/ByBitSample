using Microsoft.Extensions.Logging;

namespace MarketDataProvider
{
    public class ConnectionsFactory
    {
        public static IConnection CreateByBitConnection(ILogger? log)
        {
            return new WebSocketConnection(log, new HeartbeatFactory());
        }
    }
}
