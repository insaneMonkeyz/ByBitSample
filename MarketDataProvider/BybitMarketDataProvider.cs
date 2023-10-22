using MarketDataProvider.Bybit.Rest;
using Newtonsoft.Json;
using MarketDataProvider.Contracts.Bybit.Stream;
using System.Diagnostics;
using MarketDataProvider.BybitApi;
using MarketDataProvider.BybitApi.DTO.Rest.Market;
using MarketDataProvider.BybitApi.DTO.Rest;

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

            return securities;
        }
        public async Task SubscribeTradesAsync(ISecurity security)
        {
            throw new NotImplementedException();
        }
        public async Task UnsubscribeTradesAsync(ISecurity security)
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

        public BybitMarketDataProvider(IConnection connection)
        {
            _connection = connection;
            _dataTransmitter = connection as IDataTransmitter;
            _connection.ConnectionStateChanged += (_, state) => ConnectionStateChanged?.Invoke(this, state);
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
        private readonly Dictionary<Categories, Cache<ISecurity>> _securitiesCache = new(4);
        private readonly IList<ISecurity> _tradeSubscriptions = new List<ISecurity>(10);
        private readonly RestRequestFactory _restRequestFactory = new();
        private readonly HttpClient _restClient = new();
        private bool _disposed;
    }
}
