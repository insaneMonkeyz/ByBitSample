namespace MarketDataProvider.WebSocket
{
    public interface IAbstractWebSocketFactory
    {
        IWebSocketClient CreateWebSocketClient();
    }
}
