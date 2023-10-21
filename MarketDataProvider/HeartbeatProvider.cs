using MarketDataProvider.Contracts.Bybit.Stream;

namespace MarketDataProvider
{
    internal class HeartbeatProvider : IHeartbeatProvider
    {
        private readonly Heartbeat _msg = new();

        public bool IsHeartbeatReply(object message)
        {
            return message is Heartbeat hb
                && hb.Operation == Heartbeat.ReplyMessage;
        }
        public object GetNextMessage()
        {
            _msg.Id++;
            return _msg;
        }
    }
}
