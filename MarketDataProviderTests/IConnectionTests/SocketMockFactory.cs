using MarketDataProvider.WebSocket;
using Moq;

namespace MarketDataProviderTests.IConnectionTests
{
    public class SocketMockFactory : IAbstractWebSocketFactory
    {
        public Mock<IWebSocketClient> Mock { get; }

        public SocketMockFactory(MockBehavior mockBehavior = MockBehavior.Loose)
        {
            Mock = new Mock<IWebSocketClient>(mockBehavior);
        }
        public IWebSocketClient CreateWebSocketClient() => Mock.Object;
    }
}