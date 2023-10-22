namespace MarketDataProvider
{
    public interface ITrade
    {
        ISecurity Security { get; }
        decimal Price { get; }
        decimal Size { get; }
        DateTime Timestamp { get; }
    }
}
