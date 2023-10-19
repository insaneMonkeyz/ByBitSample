using System.Linq.Expressions;
using System.Net.WebSockets;
using MarketDataProvider;
using MarketDataProvider.WebSocket;
using Moq;

namespace MarketDataProviderTests
{
    public class SuccessfulConnectionCaseTests
    {
        ConnectionParameters _connectionParams;
        SocketMockFactory _socketMockFactory;

        Expression<Func<IWebSocketClient, Task>> DefaultConnectAsyncInvokation
        {
            get => socket => socket.ConnectAsync(It.IsNotNull<Uri>(), It.IsAny<CancellationToken>());
        }
        Expression<Func<IWebSocketClient, Task>> DefaultCloseAsyncInvokation
        {
            get =>
                socket =>
                socket.CloseAsync(
                        It.Is<WebSocketCloseStatus>(status => status == WebSocketCloseStatus.NormalClosure),
                        It.IsAny<string?>(),
                        It.IsAny<CancellationToken>());
        }

        [SetUp]
        public void Setup()
        {
            _connectionParams = new()
            {
                Uri = "wss://test.org/",
                ConnectionTimeout = TimeSpan.FromSeconds(10),
            };
            _socketMockFactory = new();

            var socketMock = _socketMockFactory.Mock;

            socketMock
                .Setup(DefaultConnectAsyncInvokation)
                .Returns(async () =>
                {
                    socketMock.Setup(s => s.State).Returns(WebSocketState.Connecting);
                    await Task.Delay(200);
                    socketMock.Setup(s => s.State).Returns(WebSocketState.Open);
                });
        }

        [Test]
        public async Task ConnectionStateOnConnected()
        {
            var connection = ConnectionsFactory.CreateByBitConnection(_socketMockFactory);
            var connectiontask = connection.ConnectAsync(_connectionParams, CancellationToken.None);

            Assert.That(connection.ConnectionState, Is.EqualTo(ConnectionState.Connecting));

            await connectiontask;

            Assert.That(connection.ConnectionState, Is.EqualTo(ConnectionState.Connected));
        }

        [Test]
        public async Task RequestsConnectionOnlyOnce()
        {
            var connection = ConnectionsFactory.CreateByBitConnection(_socketMockFactory);

            var connectiontask0 = connection.ConnectAsync(_connectionParams, CancellationToken.None);

            await Task.Delay(100);
            var connectiontask1 = connection.ConnectAsync(_connectionParams, CancellationToken.None);

            await Task.Delay(100);
            var connectiontask2 = connection.ConnectAsync(_connectionParams, CancellationToken.None);

            await Task.Delay(100);
            var connectiontask3 = connection.ConnectAsync(_connectionParams, CancellationToken.None);

            await Task.WhenAll(connectiontask0, connectiontask1, connectiontask2, connectiontask3);

            _socketMockFactory
                .Mock
                .Verify(DefaultConnectAsyncInvokation, Times.Once());
        }

        [Test]
        public async Task CanDisconnectWhenConnected()
        {
            var connection = ConnectionsFactory.CreateByBitConnection(_socketMockFactory);

            await connection.ConnectAsync(_connectionParams, CancellationToken.None);
            await connection.DisconnectAsync(CancellationToken.None);

            _socketMockFactory
                .Mock
                .Verify(DefaultCloseAsyncInvokation, Times.Once());
        }
    }
}