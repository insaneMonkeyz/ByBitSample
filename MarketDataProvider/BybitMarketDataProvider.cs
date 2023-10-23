using MarketDataProvider.Bybit.Rest;
using MarketDataProvider.BybitApi;
using MarketDataProvider.BybitApi.DTO.Rest;
using MarketDataProvider.BybitApi.DTO.Rest.Market;
using MarketDataProvider.BybitApi.DTO.Stream;
using Newtonsoft.Json;
using ZeroLog;

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

            try
            {
                var category = filter.Kind.ToBybitCategory();

                if (_securitiesCache.TryGetFreshFromCache(category, out var cached))
                {
                    return cached!.Values;
                }

                var getSecuritiesUri = _restRequestFactory.CreateGetSecuritiesRequest(category);
                var response = await RequestFromRestApi<Response<SpotSecurityDescription>>(getSecuritiesUri);
                var securities = response.ToSecurities(filter.Kind!.Value);

                if (securities is null || !securities.Any())
                {
                    return Enumerable.Empty<ISecurity>();
                }

                _securitiesCache[category].Update(securities, sec => sec.Ticker);

                return filter.TickerTemplate != null
                    ? securities.Where(s => s.Ticker.Contains(filter.TickerTemplate, StringComparison.CurrentCultureIgnoreCase))
                    : securities;
            }
            catch (Exception e)
            {
                _log.Error($"Failed to request the list of securities from the server", e);
                return Enumerable.Empty<ISecurity>();
            }
        }
        public async Task SubscribeTradesAsync(ISecurity security)
        {
            if (ConnectionState != ConnectionState.Connected)
            {
                throw new InvalidOperationException("Provider is not connected");
            }

            try
            {
                var subscribeRequest = _streamRequestFactory.CreateSubscribeTradesMessage(security.Ticker);

                await AwaitServerReply(subscribeRequest);
            }
            catch (Exception e)
            {
                _log.Error($"Could not subscribe to trades", e);
            }
        }
        public async Task UnsubscribeTradesAsync(ISecurity security)
        {
            if (ConnectionState != ConnectionState.Connected)
            {
                throw new InvalidOperationException("Provider is not connected");
            }

            try
            {
                var subscribeRequest = _streamRequestFactory.CreateUnsubscribeTradesMessage(security.Ticker);

                await AwaitServerReply(subscribeRequest);
            }
            catch (Exception e)
            {
                _log.Error($"Could not unsubscribe from trades", e);
            }
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
                _log.Debug($"Attempting to dispose an already disposed {nameof(BybitMarketDataProvider)}");
                return;
            }

            _log.Debug($"Disposing {nameof(BybitMarketDataProvider)}");

            _disposed = true;

            _connection.Dispose();
        }

        public BybitMarketDataProvider(IConnectableDataTransmitter connection)
        {
            _connection = connection as IConnection;
            _dataTransmitter = connection as IDataTransmitter;
            _msgRouter = new(_dataTransmitter);
            _msgRouter.NewTrade += OnNewTrade;
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
                    _msgRouter.RequestResponseMessage -= onServerReply;
                    serverReply.SetResult();
                }
            }

            _msgRouter.RequestResponseMessage += onServerReply;

            await _dataTransmitter.SendDataAsync(message);
            await serverReply.Task;
        }
        private void Validate(ISecurityFilter filter)
        {
            if (filter is null)
            {
                _log.Error("Trying to use security filter that is null");
                throw new ArgumentNullException(nameof(filter));
            }

            if (filter.Kind.GetValueOrDefault() == SecurityKind.Unknown)
            {
                _log.Error("Unsupported kind of security");
                throw new ArgumentException($"{nameof(filter)}.{nameof(filter.Kind)} is not set");
            }

            if (filter.EntityType is not null && filter.EntityType != TradingEntityType.Cryptocurrency)
            {
                _log.Error("Filter requires to provide an entity type that is not a cryptocurrency");
                throw new NotSupportedException("This Market Data Provider only supports cryptocurrencies");
            }
        }
        private void OnNewTrade(StreamMessage<TradeDescription> trademsg)
        {
            var trade = MessageToEntityConverter
                .Convert(trademsg, _securitiesCache.Values.FindInCache);

            if (trade != null)
            {
                try { NewTrades?.Invoke(this, trade); }
                catch { }
            }
        }
        private async Task<TExpected?> RequestFromRestApi<TExpected>(string uri)
        {
            var response = await _restClient.GetAsync(uri);
            var message = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<TExpected>(message);
            }

            _log.Error($"Requesting '{uri}' returned status code {response.StatusCode}");
            return default;
        }

        private readonly Log _log = LogManager.GetLogger<IConnection>();
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
