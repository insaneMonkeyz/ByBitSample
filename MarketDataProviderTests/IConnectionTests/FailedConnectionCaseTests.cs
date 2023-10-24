using System.Diagnostics;
using System.Linq.Expressions;
using System.Net.WebSockets;
using System.Runtime.Intrinsics.X86;
using MarketDataProvider;
using MarketDataProvider.Exceptions;
using MarketDataProvider.Factories;
using MarketDataProvider.WebSocket;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace MarketDataProviderTests.IConnectionTests
{
    public class FailedConnectionCaseTests : ConnectionTest
    {
        [SetUp]
        public void SetUp()
        {
            _connectionParams = new()
            {
                StreamHost = "wss://test.org/",
                ConnectionTimeout = TimeSpan.FromSeconds(10),
            };
            _socketMockFactory = new();

            _socketMockFactory.Mock
                .Setup(DefaultConnectAsyncInvokation)
                .Throws(new TimeoutException("connection aborted by server"));

            _connection = ConnectionsFactory.CreateByBitConnection(_socketMockFactory);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(99)]
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
        [TestCase(0, 3729)]
        [TestCase(200, 8)]
        [TestCase(5_000, 4)]
        public async Task RespectsReconnectIntervals(int reconnectIntervalMillisec, int reconnectAttempts)
        {
            _connectionParams.ReconnectionAttempts = reconnectAttempts;
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

            Assert.That(timings.Count(), Is.EqualTo(_connectionParams.ReconnectionAttempts));
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
    }
}