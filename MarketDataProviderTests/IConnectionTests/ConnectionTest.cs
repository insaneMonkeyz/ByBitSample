using System.Diagnostics;
using System.Linq.Expressions;
using System.Net.WebSockets;
using MarketDataProvider;
using MarketDataProvider.BybitApi.DTO.Stream;
using MarketDataProvider.WebSocket;
using Moq;

namespace MarketDataProviderTests.IConnectionTests
{
    public class ConnectionTest
    {
        protected IConnection _connection;
        protected ConnectionParameters _connectionParams;

        protected Mock<IHeartbeatProvider> _heartbeatFactory;
        protected Heartbeat _heartbeatMsg;

        protected TimeSpan _receiveFreezeTime = Timeout.InfiniteTimeSpan;
        protected TimeSpan _sendFreezeTime = Timeout.InfiniteTimeSpan;

        protected SocketMockFactory _socketMockFactory;

        protected Expression<Func<IWebSocketClient, Task>> DefaultCloseAsyncInvokation
        {
            get =>
                socket =>
                socket.CloseAsync(
                        It.Is<WebSocketCloseStatus>(status => status == WebSocketCloseStatus.NormalClosure),
                        It.IsAny<string?>(),
                        It.IsAny<CancellationToken>());
        }

        protected Expression<Func<IWebSocketClient, Task>> DefaultConnectAsyncInvokation
        {
            get => socket => socket.ConnectAsync(It.IsNotNull<Uri>(), It.IsAny<CancellationToken>());
        }
        protected Expression<Func<IWebSocketClient, Task>> DefaultReceiveAsyncInvokation
        {
            get => socket =>
                socket.ReceiveAsync(
                    It.IsAny<Memory<byte>>(),
                    It.IsAny<CancellationToken>());
        }
        protected Expression<Func<IWebSocketClient, Task>> DefaultSendAsyncInvokation
        {
            get => socket
                => socket.SendAsync(
                    It.IsAny<ReadOnlyMemory<byte>>(),
                    It.Is<WebSocketMessageType>(mt => mt == WebSocketMessageType.Text),
                    It.IsAny<WebSocketMessageFlags>(),
                    It.IsAny<CancellationToken>());
        }

        protected async Task ReceiveAsyncFreeze(Memory<byte> _, CancellationToken cancellation)
        {
            await Task.Delay(_receiveFreezeTime, cancellation);
        }

        protected async Task SendAsyncFreeze(
            ReadOnlyMemory<byte> buffer,
                WebSocketMessageType msgType,
                    WebSocketMessageFlags msgFlags,
                        CancellationToken cancellation)
        {
            await Task.Delay(_sendFreezeTime, cancellation);
        }

        [TearDown]
        public void CleanUp()
        {
            _connection?.Dispose();
        }

        protected async Task<IEnumerable<double>> MeasureTime(Func<Action, Action, Task> measuredSubject)
        {
            var timer = new Stopwatch();
            var timings = new List<double>();

            void registerTime()
            {
                timings.Add(Math.Max(timer.ElapsedMilliseconds, 1));
                timer.Restart();
            }

            await measuredSubject(timer.Start, registerTime);

            // first result is a regular connect attempt,
            // not a reconnect
            timings.RemoveAt(0);

            return timings;
        }

        protected void SetupSeccussfulConnectAsync()
        {
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

        protected void SetupHeartbeatLogic()
        {
            _heartbeatFactory = new Mock<IHeartbeatProvider>();
            _heartbeatMsg = new();
            _heartbeatFactory
                .Setup(f => f.GetNextMessage())
                .Callback(() => ++_heartbeatMsg.Id)
                .Returns(() => _heartbeatMsg);

        }

        protected void SetupReceiveAsyncFreeze(TimeSpan freezeTime)
        {
            _receiveFreezeTime = freezeTime;

            _socketMockFactory.Mock
                .Setup(DefaultReceiveAsyncInvokation)
                .Returns(ReceiveAsyncFreeze);
        }

        protected void SetupSendAsyncFreeze(TimeSpan freezeTime)
        {
            _sendFreezeTime = freezeTime;

            _socketMockFactory.Mock
                .Setup(DefaultSendAsyncInvokation)
                .Returns(SendAsyncFreeze);
        }
    }
}