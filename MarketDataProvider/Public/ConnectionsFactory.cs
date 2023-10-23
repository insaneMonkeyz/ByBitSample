using MarketDataProvider.WebSocket;

namespace MarketDataProvider
{
    public class ConnectionsFactory
    {
        public static IConnectableDataTransmitter CreateByBitConnection()
        {
            return new WebSocketConnection(new HeartbeatProvider(new()), new WebSocketClientFactory());
        }
        public static IConnectableDataTransmitter CreateByBitConnection(IAbstractWebSocketFactory socketFactory)
        {
            return new WebSocketConnection(new HeartbeatProvider(new()), socketFactory);
        }
        public static IConnectableDataTransmitter CreateByBitConnection(IAbstractWebSocketFactory socketFactory, IHeartbeatProvider heartbeatFactory)
        {
            return new WebSocketConnection(heartbeatFactory, socketFactory);
        }
    }
}
