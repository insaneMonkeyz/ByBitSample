using Newtonsoft.Json;

namespace MarketDataProvider.BybitApi.DTO.Rest.Market
{
    [Serializable]
    public class SecurityDescription
    {
        [JsonProperty("symbol")]
        public string? Ticker { get; set; }

        [JsonProperty("bid1Price")]
        public decimal? BidPrice { get; set; }

        [JsonProperty("bid1Size")]
        public decimal? BidSize { get; set; }

        [JsonProperty("ask1Price")]
        public decimal? AskPrice { get; set; }

        [JsonProperty("ask1Size")]
        public decimal? AskSize { get; set; }

        [JsonProperty("lastPrice")]
        public decimal? LastPrice { get; set; }

        [JsonProperty("prevPrice24h")]
        public decimal? YesterdayPrice { get; set; }

        [JsonProperty("highPrice24h")]
        public decimal? DailyHighestPrice { get; set; }

        [JsonProperty("lowPrice24h")]
        public decimal? DailyLowestPrice { get; set; }

        [JsonProperty("price24hPcnt")]
        public double? DailyPriceChange { get; set; }

        [JsonProperty("volume24h")]
        public decimal? DailyVolume { get; set; }

        [JsonProperty("turnover24h")]
        public decimal? DailyTurnover { get; set; }
    }
}