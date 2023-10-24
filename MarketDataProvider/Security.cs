namespace MarketDataProvider
{
    internal class Security : ISecurity
    {
        public required string Ticker { get; init; }
        public required SecurityKind Kind { get; init; }
        public required TradingEntityType EntityType { get; init; }

        public override string ToString()
        {
            return $"{Kind} {Ticker}";
        }
    }
}
