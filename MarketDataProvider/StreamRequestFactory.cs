using MarketDataProvider.BybitApi;
using MarketDataProvider.BybitApi.DTO.Stream;

namespace MarketDataProvider.Bybit.Rest
{
    internal class StreamRequestFactory
    {
        private long _lastRequestId = 0;

        public RequestMessage CreateHeartbeatMessage()
        {
            return new RequestMessage
            {
                Id = (_lastRequestId++).ToString(),
                Operation = StreamOperations.Heartbeat
            };
        }

        public RequestMessage CreateSubscribeTradesMessage(string ticker)
        {
            return new RequestMessage
            {
                Id = (_lastRequestId++).ToString(),
                Operation = StreamOperations.Subscribe,
                Args = new object[] { $"{StreamTopics.Trades}.{ticker}" } 
            };
        }

        public RequestMessage CreateUnsubscribeTradesMessage(string ticker)
        {
            return new RequestMessage
            {
                Id = (_lastRequestId++).ToString(),
                Operation = StreamOperations.Unsubscribe,
                Args = new object[] { $"{StreamTopics.Trades}.{ticker}" }
            };
        }
    }
}
