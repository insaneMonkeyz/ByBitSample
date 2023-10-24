using System.Text;
using System.Text.Json;
using MarketDataProvider;
using MarketDataProvider.Factories;
using Moq;

namespace MarketDataProviderTests.IConnectionTests
{
    public class DataExchangeTests : ConnectionTest
    {
        private IDataTransmitter _dataTransmitter;

        [SetUp]
        public void SetUp()
        {
            _connectionParams = new()
            {
                StreamHost = "wss://test.org/",
                ConnectionTimeout = TimeSpan.FromSeconds(10),
                UseHeartbeating = false,
            };

            _socketMockFactory = new();
            _connection = ConnectionsFactory.CreateByBitConnection(_socketMockFactory);
            _dataTransmitter = _connection as IDataTransmitter;

            SetupSeccussfulConnectAsync();
        }

        [Test]
        [Description("The user of the IConnection shoud not receive heartbeat messages as they are service messages")]
        public async Task ServerPingMsgsNotForwardedTest()
        {
            _socketMockFactory.Mock
                .Setup(DefaultReceiveAsyncInvokation)
                .Callback(async (Memory<byte> buffer, CancellationToken _) =>
                {
                    await Task.Run(() =>
                    {
                        var reply = $$"""
                        {
                            "req_id": 88005553535,
                            "op": "ping"
                        }
                        """;

                        buffer = Encoding.UTF8.GetBytes(reply);

                        SetupReceiveAsyncFreeze(Timeout.InfiniteTimeSpan);
                    });
                });

            _dataTransmitter.ServerReply += (_, _) => Assert.Fail();

            await _connection.ConnectAsync(_connectionParams, CancellationToken.None);

            _socketMockFactory.Mock.Verify(DefaultReceiveAsyncInvokation, Times.Once());
        }

        [Test]
        public async Task ServerReplyEventIsRaisedTest()
        {
            var testmsg = new TestMessage()
            {
                IntegerProperty = 666,
                StringProperty = "wobblabobbla"
            };

            _socketMockFactory.Mock
                .Setup(DefaultReceiveAsyncInvokation)
                .Callback(async (Memory<byte> buffer, CancellationToken _) =>
                {
                    await Task.Run(() =>
                    {
                        var serialized = JsonSerializer.Serialize(testmsg);
                        buffer = Encoding.UTF8.GetBytes(serialized);

                        SetupReceiveAsyncFreeze(Timeout.InfiniteTimeSpan);
                    });
                });

            _dataTransmitter.ServerReply += (_, data) => Assert.That(data.Equals(testmsg));

            await _connection.ConnectAsync(_connectionParams, CancellationToken.None);
            await Task.Delay(100);
        }
    }
}