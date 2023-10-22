using MarketDataProvider.Bybit.Rest;
using MarketDataProvider.BybitApi;
using MarketDataProvider.BybitApi.DTO.Rest;
using MarketDataProvider.BybitApi.DTO.Rest.Market;
using MarketDataProvider.BybitApi.DTO.Stream;
using Newtonsoft.Json;

namespace MarketDataProvider
{
    public class BybitMarketDataProvider : IMarketDataProvider, IConnection
    {
        public event EventHandler<ITrade>? NewTrades;
        public event EventHandler<ConnectionState>? ConnectionStateChanged;
        public ConnectionState ConnectionState => _connection.ConnectionState;

        public async Task<IEnumerable<ISecurity>> GetAvailablSecuritiesAsync(ISecurityFilter filter)
        {
            Validate(filter);

            var category = filter.Kind.ToBybitCategory();

            if(_securitiesCache.TryGetFreshFromCache(category, out var cached))
            {
                return cached;
            }

            var getSecuritiesUri = _restRequestFactory.CreateGetSecuritiesRequest(category);
            var response = await RequestFromRestApi<Response<SpotSecurityDescription>>(getSecuritiesUri);
            var securities = response.ToSecurities(filter.Kind!.Value);

            _securitiesCache[category].Update(securities);

            return filter.TickerTemplate != null
                ? securities.Where(s => s.Ticker.Contains(filter.TickerTemplate, StringComparison.CurrentCultureIgnoreCase))
                : securities;
        }
        public async Task SubscribeTradesAsync(ISecurity security)
        {
            if (ConnectionState != ConnectionState.Connected)
            {
                throw new InvalidOperationException("Provider is not connected");
            }

            var subscribeRequest = _streamRequestFactory.CreateSubscribeMessage(security.Ticker);

            await AwaitServerReply(subscribeRequest);
        }
        public async Task UnsubscribeTradesAsync(ISecurity security)
        {
            if (ConnectionState != ConnectionState.Connected)
            {
                throw new InvalidOperationException("Provider is not connected");
            }

            var subscribeRequest = _streamRequestFactory.CreateUnsubscribeMessage(security.Ticker);

            await AwaitServerReply(subscribeRequest);
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

        public BybitMarketDataProvider(IConnectableDataTransmitter connection)
        {
            _connection = connection as IConnection;
            _dataTransmitter = connection as IDataTransmitter;
            _msgRouter = new(_dataTransmitter);
            _connection.ConnectionStateChanged += (_, state) => ConnectionStateChanged?.Invoke(this, state);
        }

        private async Task AwaitServerReply(RequestMessage message)
        {
            var serverReply = new TaskCompletionSource();
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            cancellation.Token.Register(serverReply.SetCanceled);

            void onServerReply(RequestResponseMessage response)
            {
                if (message.Id!.Equals(response.Id))
                {
                    _msgRouter.OnRequestResponseMessage -= onServerReply;
                    serverReply.SetResult();
                }
            }

            _msgRouter.OnRequestResponseMessage += onServerReply;

            await _dataTransmitter.SendDataAsync(message);
            await serverReply.Task;
        }
        private static void Validate(ISecurityFilter filter)
        {
            if (filter is null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            if (filter.Kind.GetValueOrDefault() == SecurityKind.Unknown)
            {
                throw new ArgumentException($"{nameof(filter)}.{nameof(filter.Kind)} is not set");
            }

            if (filter.EntityType is not null && filter.EntityType != TradingEntityType.Cryptocurrency)
            {
                throw new NotSupportedException("This Market Data Provider only supports cryptocurrencies");
            }
        }
        private async Task<TExpected?> RequestFromRestApi<TExpected>(string uri)
        {
            try
            {
                var response = await _restClient.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var message = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<TExpected>(message);
                }
            }
            catch (Exception)
            {
            }

            return default;
        }

        private readonly IConnection _connection;
        private readonly IDataTransmitter _dataTransmitter;
        private readonly BybitMessageRouter _msgRouter;
        private readonly IList<ISecurity> _tradeSubscriptions = new List<ISecurity>(10);
        private readonly RestRequestFactory _restRequestFactory = new();
        private readonly StreamRequestFactory _streamRequestFactory = new();
        private readonly Dictionary<Categories, Cache<ISecurity>> _securitiesCache = new(4);
        private readonly HttpClient _restClient = new();
        private bool _disposed;
    }
}
