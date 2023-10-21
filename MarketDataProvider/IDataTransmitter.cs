namespace MarketDataProvider
{
    public interface IDataTransmitter
    {
        event EventHandler<object> ServerReply;
        Task SendDataAsync(object data);
    }
}
