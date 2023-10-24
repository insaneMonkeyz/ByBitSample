namespace MarketDataProvider
{
    internal class Trade : ITrade
    {
        public required ISecurity Security { get; init; }
        public Sides Side { get; init; }
        public decimal Price { get; init; }
        public decimal Size { get; init; }
        public DateTime Timestamp { get; init; }

        public override string ToString()
        {
            return $"{Timestamp:O} {Security} {Side} {Size} x {Price}";
        }
    }
}
