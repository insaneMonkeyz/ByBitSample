using System.Diagnostics;
using System.Linq.Expressions;
using System.Net.WebSockets;
using System.Runtime.Intrinsics.X86;
using MarketDataProvider;
using MarketDataProvider.Exceptions;
using MarketDataProvider.WebSocket;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace MarketDataProviderTests
{
    public class FailedConnectionCaseTests
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
        public void SetUp()
        {
            _connectionParams = new()
            {
                Uri = "wss://test.org/",
                ConnectionTimeout = TimeSpan.FromSeconds(10),
            };
            _socketMockFactory = new();
            _connection = ConnectionsFactory.CreateByBitConnection(_socketMockFactory);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(4)]
        [TestCase(9)]
        public async Task ReconnectsOnConnectionFailure(int reconnectAttemptsNum)
        {
            _connectionParams.ReconnectionAttempts = reconnectAttemptsNum;

            try
            {
                await _connection.ConnectAsync(_connectionParams, CancellationToken.None);
            }
            catch (Exception) { }

            _socketMockFactory
                .Mock                                  // one initial call + reconnection attempts
                .Verify(DefaultConnectAsyncInvokation, Times.Exactly(reconnectAttemptsNum + 1));
        }

        [Test]
        public async Task ThrowsWhenFailedToConnect()
        {
            try
            {
                await _connection.ConnectAsync(_connectionParams, CancellationToken.None);
                Assert.Fail("Exception was not thrown");
            }
            catch (ConnectionException)
            {
                Assert.Pass();
            }
            catch
            {
                Assert.Fail($"Exception exception of type {typeof(ConnectionException)}");
            }
        }

        [Test]
        [Description("Assert that nothing forbids the user to request connection again after previous attempt failed")]
        public async Task CanRequestConnectionAfterFail()
        {
            try
            {
                await _connection.ConnectAsync(_connectionParams, CancellationToken.None);
            }
            catch (Exception) { }

            try
            {
                await _connection.ConnectAsync(_connectionParams, CancellationToken.None);
            }
            catch (Exception) { }

            _socketMockFactory.Mock
                .Verify(DefaultConnectAsyncInvokation, Times.Exactly(2));
        }

        [Test]
        public async Task StatusDisconnectedAfterFailedConnection()
        {
            try
            {
                await _connection.ConnectAsync(_connectionParams, CancellationToken.None);
            }
            catch (Exception) { }

            Assert.That(_connection.ConnectionState, Is.EqualTo(ConnectionState.Disconnected));
        }

        [Test]
        [TestCase(0)]
        [TestCase(200)]
        [TestCase(1_000)]
        [TestCase(5_000)]
        [TestCase(20_000)]
        public async Task RespectsReconnectIntervals(int reconnectIntervalMillisec)
        {
            _connectionParams.ReconnectionAttempts = 5;
            _connectionParams.ReconnectionInterval = TimeSpan.FromMilliseconds(reconnectIntervalMillisec);

            var measuretask = MeasureTime(async (beginMeasuring, registerTime) =>
            {
                _socketMockFactory.Mock
                    .Setup(DefaultConnectAsyncInvokation)
                    .Callback(registerTime);

                try
                {
                    beginMeasuring();
                    await _connection.ConnectAsync(_connectionParams, CancellationToken.None);
                }
                catch (Exception) { }
            });

            var timings = await measuretask;

            Assert.That(IsWithinErrorRange(reconnectIntervalMillisec, timings));
        }

        private bool IsWithinErrorRange(double expectedAverageValue, IEnumerable<double> timings)
        {
            // minimal time required to reconnect when no reconnect interval is set
            const int MinimumDelayValueMillisec = 200;
            const double ReconnectTimeErrorPercentage = 0.1d;

            expectedAverageValue = Math.Max(MinimumDelayValueMillisec, expectedAverageValue);

            var actualAverage = timings.Average();

            // lets pretend this does not need its own test, kek
            var standardDeviation =
                Math.Sqrt(
                    timings.Average(avgT =>
                        Math.Pow(avgT - actualAverage, 2)));

            var expectedErrorValue = Math.Abs(expectedAverageValue * ReconnectTimeErrorPercentage);
            var actualDifference = actualAverage - expectedAverageValue;

            return actualDifference < expectedErrorValue && standardDeviation < expectedErrorValue;
        }

        private async Task<IEnumerable<double>> MeasureTime(Func<Action, Action, Task> measuredSubject)
        {
            var timer = new Stopwatch();
            var timings = new List<double>();

            void registerTime()
            {
                timings.Add(timer.ElapsedMilliseconds);
                timer.Restart();
            }

            await measuredSubject(timer.Start, registerTime);

            // first result is a regular connect attempt,
            // not a reconnect
            timings.RemoveAt(0);

            return timings.Where(t => t != default);
        }
    }
}