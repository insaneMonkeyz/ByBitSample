using Newtonsoft.Json;

namespace MarketDataProvider.BybitApi.DTO.Stream
{
    [Serializable]
    public class SpotUpdate : SecurityUpdate
    {
        [JsonProperty("prevPrice24h")]
        public decimal? YesterdayPrice { get; set; }

        [JsonProperty("price24hPcnt")]
        public double? DailyPriceChange { get; set; }

        [JsonProperty("usdIndexPrice")]
        public decimal? UsdIndex { get; set; }
    }
}