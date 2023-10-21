namespace MarketDataProvider
{
    internal class MarketDataProvider : IMarketDataProvider, IConnection
    {
        public event EventHandler<object>? NewTrades;
        public event EventHandler<ConnectionState>? ConnectionStateChanged;
        public ConnectionState ConnectionState => _connection.ConnectionState;

        public async Task<object> GetAvailablSecuritiesAsync()
        {
            throw new NotImplementedException();
        }
        public async Task SubscribeTradesAsync(string ticker)
        {
            throw new NotImplementedException();
        }
        public async Task UnsubscribeTradesAsync(string ticker)
        {
            throw new NotImplementedException();
        }
        public async Task ConnectAsync(ConnectionParameters parameters, CancellationToken cancellationToken)
        {
            await _connection.ConnectAsync(parameters, cancellationToken);
        }
        public async Task DisconnectAsync(CancellationToken cancellationToken)
        {
            await _connection.DisconnectAsync(cancellationToken);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            _connection.Dispose();
        }

        public MarketDataProvider(IConnection connection)
        {
            _connection = connection;
            _dataTransmitter = connection as IDataTransmitter;
            _connection.ConnectionStateChanged += (_, state) => ConnectionStateChanged?.Invoke(this, state);
        }

        private IConnection _connection;
        private IDataTransmitter _dataTransmitter;
        private bool _disposed;
    }
}
