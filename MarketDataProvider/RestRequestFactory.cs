using MarketDataProvider.BybitApi;

namespace MarketDataProvider.Bybit.Rest
{

    internal class RestRequestFactory
    {
        public string HostAddress { get; init; } = "https://api-testnet.bybit.com";
        public string GetSecuritiesPath { get; init; } = "v5/market/tickers";

        private string? _getSecuritiesAddress;
        private string GetSecuritiesAddress
	    {
            get
		    {
                if (_getSecuritiesAddress is not null)
                {
                    return _getSecuritiesAddress;
                }

                var uribuilder = new UriBuilder(HostAddress)
                {
                    Path = GetSecuritiesPath,
                };

                _getSecuritiesAddress = uribuilder.Uri.ToString();

                return _getSecuritiesAddress;
            } 
        }

        public string CreateGetOptionsRequest(string underlying, DateOnly? expiry = null)
        {
            if (underlying is null)
            {
                throw new ArgumentNullException(nameof(underlying));
            }

            if (expiry is DateOnly exp)
            {
                return $"{GetSecuritiesAddress}?" +
                    $"category={Categories.Option.ToBybitCategory()}&" +
                    $"expDate={exp.ToBybitDateFormat()}&" +
                    $"baseCoin={underlying}";
            }
            else
            {
                return $"{GetSecuritiesAddress}?" +
                    $"category={Categories.Option.ToBybitCategory()}&" +
                    $"baseCoin={underlying}";
            }
        }

        public string CreateGetSecuritiesRequest(Categories securityType, string? ticker = null)
        {
            return ticker is null
                ? $"{GetSecuritiesAddress}?category={securityType.ToBybitCategory()}"
                : $"{GetSecuritiesAddress}?category={securityType.ToBybitCategory()}&symbol={ticker}";
        }
    }
}
