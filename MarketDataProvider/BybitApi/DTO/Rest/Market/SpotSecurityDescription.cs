using Newtonsoft.Json;

namespace MarketDataProvider.BybitApi.DTO.Rest.Market
{
    [Serializable]
    public class SpotSecurityDescription : SecurityDescription
    {
        [JsonProperty("usdIndexPrice")]
        public decimal? UsdIndex { get; set; }
    }
}
