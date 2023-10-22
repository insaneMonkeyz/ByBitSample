using MarketDataProvider.Contracts.Bybit;
using MarketDataProvider.WebSocket;
using Microsoft.Extensions.Logging;

namespace MarketDataProvider
{
    public class ConnectionsFactory
    {
        public static IConnection CreateByBitConnection()
        {
            return new WebSocketConnection(null, new HeartbeatProvider(), new WebSocketClientFactory());
        }
        public static IConnection CreateByBitConnection(IAbstractWebSocketFactory socketFactory)
        {
            return new WebSocketConnection(null, new HeartbeatProvider(), socketFactory);
        }
        public static IConnection CreateByBitConnection(IAbstractWebSocketFactory socketFactory, IHeartbeatProvider heartbeatFactory)
        {
            return new WebSocketConnection(null, heartbeatFactory, socketFactory);
        }
    }
}
