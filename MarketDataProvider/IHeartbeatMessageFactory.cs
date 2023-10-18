namespace MarketDataProvider
{
    internal interface IHeartbeatMessageFactory
    {
        object GetNextMessage();
    }
}
