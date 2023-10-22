using Newtonsoft.Json;

namespace MarketDataProvider.BybitApi.DTO.Stream
{
    [Serializable]
    public class OptionUpdate : SecurityUpdate
    {
        [JsonProperty("change24h")]
        public decimal? DailyPriceChange { get; set; }

        [JsonProperty("predictedDeliveryPrice")]
        public decimal? ExpectedDeliveryPrice { get; set; }

        public decimal? BidPrice { get; set; }
        public decimal? BidSize { get; set; }
        public decimal? BidIv { get; set; }

        public decimal? AskPrice { get; set; }
        public decimal? AskSize { get; set; }
        public decimal? AskIv { get; set; }

        public decimal? Delta { get; set; }
        public decimal? Vega { get; set; }
        public decimal? Gamma { get; set; }
        public decimal? Theta { get; set; }

        public decimal? MarkPrice { get; set; }
        public decimal? MarkPriceIv { get; set; }
        public decimal? IndexPrice { get; set; }
        public decimal? UnderlyingPrice { get; set; }
        public decimal? TotalVolume { get; set; }
        public decimal? TotalTurnover { get; set; }
        public long? OpenInterest { get; set; }
    }
}