namespace MarketDataProvider
{
    public interface IConnection : IDisposable
    {
        event EventHandler<ConnectionState> ConnectionStateChanged;
        ConnectionState ConnectionState { get; }
        Task ConnectAsync(ConnectionParameters parameters, CancellationToken cancellationToken);
        Task DisconnectAsync(CancellationToken cancellationToken);
    }
}
