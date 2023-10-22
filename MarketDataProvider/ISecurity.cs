namespace MarketDataProvider
{
    public interface ISecurity
    {
        string Ticker { get; set; }
        SecurityKind Kind { get; }
        TradingEntityType EntityType { get; }
    }
}
