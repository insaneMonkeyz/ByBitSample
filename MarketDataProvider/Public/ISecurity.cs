namespace MarketDataProvider
{
    public interface ISecurity
    {
        string Ticker { get; }
        SecurityKind Kind { get; }
        TradingEntityType EntityType { get; }
    }
}
