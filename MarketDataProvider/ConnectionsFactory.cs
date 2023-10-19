using MarketDataProvider.WebSocket;
using Microsoft.Extensions.Logging;

namespace MarketDataProvider
{
    public class ConnectionsFactory
    {
        public static IConnection CreateByBitConnection()
        {
            return new WebSocketConnection(null, new HeartbeatFactory(), new WebSocketClientFactory());
        }
        public static IConnection CreateByBitConnection(IAbstractWebSocketFactory socketFactory)
        {
            return new WebSocketConnection(null, new HeartbeatFactory(), socketFactory);
        }
    }
}
