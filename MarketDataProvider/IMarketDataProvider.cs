namespace MarketDataProvider
{
    public interface IMarketDataProvider
    {
        event EventHandler<object> NewTrades;
        Task<object> GetAvailablSecuritiesAsync();
        Task SubscribeTradesAsync(string ticker);
        Task UnsubscribeTradesAsync(string ticker);
    }
}
