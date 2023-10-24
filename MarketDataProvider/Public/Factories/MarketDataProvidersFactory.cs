namespace MarketDataProvider.Factories
{
    public class MarketDataProvidersFactory
    {
        public static IMarketDataProvider CreateBybitProvider(IConnectableDataTransmitter connection)
        {
            return new BybitMarketDataProvider(connection ?? throw new ArgumentException(nameof(connection)));
        }
    }
}
