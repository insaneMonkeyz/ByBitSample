namespace MarketDataProvider
{
    public interface IHeartbeatProvider
    {
        bool IsHeartbeatReply(object message);
        object GetNextMessage();
    }
}
