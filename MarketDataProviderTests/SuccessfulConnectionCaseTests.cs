using System.Data.Common;
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
        IConnection _connection;

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
            _connection = ConnectionsFactory.CreateByBitConnection(_socketMockFactory);

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
            var connectiontask = _connection.ConnectAsync(_connectionParams, CancellationToken.None);

            Assert.That(_connection.ConnectionState, Is.EqualTo(ConnectionState.Connecting));

            await connectiontask;

            Assert.That(_connection.ConnectionState, Is.EqualTo(ConnectionState.Connected));
        }

        [Test]
        public async Task RequestsConnectionOnlyOnce()
        {
            var connectiontask0 = _connection.ConnectAsync(_connectionParams, CancellationToken.None);

            await Task.Delay(100);
            var connectiontask1 = _connection.ConnectAsync(_connectionParams, CancellationToken.None);

            await Task.Delay(100);
            var connectiontask2 = _connection.ConnectAsync(_connectionParams, CancellationToken.None);

            await Task.Delay(100);
            var connectiontask3 = _connection.ConnectAsync(_connectionParams, CancellationToken.None);

            await Task.WhenAll(connectiontask0, connectiontask1, connectiontask2, connectiontask3);

            _socketMockFactory
                .Mock
                .Verify(DefaultConnectAsyncInvokation, Times.Once());
        }

        [Test]
        public async Task CanDisconnectWhenConnected()
        {
            await _connection.ConnectAsync(_connectionParams, CancellationToken.None);
            await _connection.DisconnectAsync(CancellationToken.None);

            _socketMockFactory
                .Mock
                .Verify(DefaultCloseAsyncInvokation, Times.Once());
        }
    }
}