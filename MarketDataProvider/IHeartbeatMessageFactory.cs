namespace MarketDataProvider
{
    public interface IHeartbeatMessageFactory
    {
        object GetNextMessage();
    }
}
