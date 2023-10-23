using MarketDataProvider;

namespace ConsoleUI;

class SecurityFilter : ISecurityFilter
{
    public string? TickerTemplate { get; set; }
    public SecurityKind? Kind { get; set; }
    public TradingEntityType? EntityType { get; set; }
}