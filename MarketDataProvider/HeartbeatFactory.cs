using MarketDataProvider.Contracts.ByBit;

namespace MarketDataProvider
{
    internal class HeartbeatFactory : IHeartbeatMessageFactory
    {
        private readonly Heartbeat _msg = new();

        public object GetNextMessage()
        {
            _msg.Id++;
            return _msg;
        }
    }
}
