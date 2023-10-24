namespace MarketDataProvider
{
    public interface ITrade
    {
        ISecurity Security { get; }
        Sides Side { get; }
        decimal Price { get; }
        decimal Size { get; }
        DateTime Timestamp { get; }
    }
}
