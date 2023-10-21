using Newtonsoft.Json;

namespace MarketDataProvider.Contracts.Bybit.Stream
{
    [Serializable]
    public class SecurityUpdate
    {
        [JsonProperty("highPrice24h")]
        public decimal? DailyHighestPrice { get; set; }

        [JsonProperty("lowPrice24h")]
        public decimal? DailyLowestPrice { get; set; }

        [JsonProperty("turnover24h")]
        public decimal? DailyTurnover { get; set; }

        [JsonProperty("volume24h")]
        public decimal? DailyVolume { get; set; }

        [JsonProperty("lastPrice")]
        public decimal? LastPrice { get; set; }

        [JsonProperty("symbol")]
        public string? Ticker { get; set; }
    }
}