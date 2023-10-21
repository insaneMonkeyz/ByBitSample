namespace MarketDataProvider
{
    public interface IConnection : IDisposable
    {
        event EventHandler<object> ServerReply;
        event EventHandler<ConnectionState> ConnectionStateChanged;
        ConnectionState ConnectionState { get; }
        Task SendDataAsync(object data);
        Task ConnectAsync(ConnectionParameters parameters, CancellationToken cancellationToken);
        Task DisconnectAsync(CancellationToken cancellationToken);
    }
}
