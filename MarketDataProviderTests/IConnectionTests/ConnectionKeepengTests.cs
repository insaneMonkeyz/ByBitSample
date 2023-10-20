using System.Linq.Expressions;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using MarketDataProvider;
using MarketDataProvider.Contracts.ByBit;
using MarketDataProvider.WebSocket;
using Moq;

namespace MarketDataProviderTests.IConnectionTests
{
    public class ConnectionKeepingTests : ConnectionTest
    {
        [SetUp]
        public void SetUp()
        {
            _connectionParams = new()
            {
                Uri = "wss://test.org/",
                ConnectionTimeout = Timeout.InfiniteTimeSpan,
                HeartbeatInterval = TimeSpan.FromMilliseconds(100),
                UseHeartbeating = true,
            };

            _socketMockFactory = new();

            _heartbeatFactory = new Mock<IHeartbeatMessageFactory>();
            _heartbeatMsg = new();
            _heartbeatFactory
                .Setup(f => f.GetNextMessage())
                .Callback(() => ++_heartbeatMsg.Id)
                .Returns(() => _heartbeatMsg);

            _connection = ConnectionsFactory.CreateByBitConnection(_socketMockFactory, _heartbeatFactory.Object);

            var socketMock = _socketMockFactory.Mock;

            socketMock
                .Setup(DefaultConnectAsyncInvokation)
                .Returns(async () =>
                {
                    socketMock.Setup(s => s.State).Returns(WebSocketState.Connecting);
                    await Task.Delay(50);
                    socketMock.Setup(s => s.State).Returns(WebSocketState.Open);
                });

            _receiveFreezeTime = TimeSpan.FromMilliseconds(30);

            socketMock
                .Setup(DefaultReceiveAsyncInvokation)
                .Returns(ReceiveAsyncFreeze);

            _sendFreezeTime = TimeSpan.FromMilliseconds(30);

            socketMock
                .Setup(DefaultSendAsyncInvokation)
                .Returns(SendAsyncFreeze);
        }

        [Test]
        [TestCase(100, 69)]
        [TestCase(500, 17)]
        [TestCase(2000, 4)]
        public async Task HeartbeatFrequencyTest(int heartbeatIntervalMillisec, int expectedHeartbeatsNum)
        {
            _connectionParams.HeartbeatInterval = TimeSpan.FromMilliseconds(heartbeatIntervalMillisec);

            await _connection.ConnectAsync(_connectionParams, CancellationToken.None);

            var expectedDelay = heartbeatIntervalMillisec * expectedHeartbeatsNum;

            await Task.Delay(expectedDelay);

            _socketMockFactory.Mock
                .Verify(DefaultSendAsyncInvokation, Times.Exactly(expectedHeartbeatsNum));
        }

        [Test]
        public async Task SendsExpectedHeartbeatTypeTest()
        {
            await _connection.ConnectAsync(_connectionParams, CancellationToken.None);
            await Task.Delay(_connectionParams.HeartbeatInterval);

            var expectedMsg = JsonSerializer.Serialize(_heartbeatMsg);

            // verify that socket.SendAsync is called only once and the heartbeat msg is passed
            _socketMockFactory.Mock
                .Verify(

                    socket => socket.SendAsync(
                        It.Is<ReadOnlyMemory<byte>>(

                            buffer => Encoding.UTF8
                                        .GetString(buffer.ToArray())
                                        .Equals(expectedMsg)),

                        It.Is<WebSocketMessageType>(mt => mt == WebSocketMessageType.Text),
                        It.IsAny<WebSocketMessageFlags>(),
                        It.IsAny<CancellationToken>()),

                    Times.Once());
        }

        [Test]
        [TestCase(100)]
        [TestCase(2000)]
        [TestCase(15000)]
        public async Task DisconnectsWhenServerIsSilent(int connectionTimeoutMillisec)
        {
            _connectionParams.UseHeartbeating = false;
            _connectionParams.ConnectionTimeout = TimeSpan.FromMilliseconds(connectionTimeoutMillisec);

            _receiveFreezeTime = Timeout.InfiniteTimeSpan;

            _socketMockFactory.Mock
                .Setup(DefaultReceiveAsyncInvokation)
                .Returns(ReceiveAsyncFreeze);

            await _connection.ConnectAsync(_connectionParams, CancellationToken.None);
            await Task.Delay(connectionTimeoutMillisec + 50); //additional time to let the operation finish

            _socketMockFactory.Mock.Verify(DefaultCloseAsyncInvokation, Times.Once());
        }
    }
}
