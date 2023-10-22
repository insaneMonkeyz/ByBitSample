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

        public RequestMessage CreateSubscribeMessage(string ticker)
        {
            return new RequestMessage
            {
                Id = (_lastRequestId++).ToString(),
                Operation = StreamOperations.Subscribe,
                Args = new object[] { $"tickers.{ticker}" } 
            };
        }

        public RequestMessage CreateUnsubscribeMessage(string ticker)
        {
            return new RequestMessage
            {
                Id = (_lastRequestId++).ToString(),
                Operation = StreamOperations.Unsubscribe,
                Args = new object[] { $"tickers.{ticker}" }
            };
        }
    }
}
