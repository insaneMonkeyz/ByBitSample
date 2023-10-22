namespace MarketDataProvider
{
    public interface ISecurityFilter
    {
        string? TickerTemplate { get; set; }
        SecurityKind? Kind { get; set; }
        TradingEntityType? EntityType { get; set; }
    }
}
