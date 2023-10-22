namespace MarketDataProvider
{
    public interface IMarketDataProvider
    {
        event EventHandler<ITrade> NewTrades;
        Task<IEnumerable<ISecurity>> GetAvailablSecuritiesAsync(ISecurityFilter filter);
        Task SubscribeTradesAsync(ISecurity security);
        Task UnsubscribeTradesAsync(ISecurity security);
    }
}
