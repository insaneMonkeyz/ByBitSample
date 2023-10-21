using Newtonsoft.Json;

namespace MarketDataProvider.Contracts.Bybit.Rest.Market
{
    [Serializable]
    public class SpotSecurityDescription : SecurityDescription
    {
        [JsonProperty("usdIndexPrice")]
        public decimal? UsdIndex { get; set; }
    }
}
