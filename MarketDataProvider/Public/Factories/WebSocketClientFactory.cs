using MarketDataProvider.WebSocket;

namespace MarketDataProvider.Factories
{
    public class WebSocketClientFactory : IAbstractWebSocketFactory
    {
        public IWebSocketClient CreateWebSocketClient() => new WebSocketAdapter();
    }
}
