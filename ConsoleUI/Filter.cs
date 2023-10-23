using MarketDataProvider;

namespace ConsoleUI;

internal partial class Program
{
    class Filter : ISecurityFilter
    {
        public string? TickerTemplate { get; set; }
        public SecurityKind? Kind { get; set; }
        public TradingEntityType? EntityType { get; set; }
    }
}