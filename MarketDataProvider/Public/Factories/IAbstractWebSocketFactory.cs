using MarketDataProvider.WebSocket;

namespace MarketDataProvider.Factories
{
    public interface IAbstractWebSocketFactory
    {
        IWebSocketClient CreateWebSocketClient();
    }
}
