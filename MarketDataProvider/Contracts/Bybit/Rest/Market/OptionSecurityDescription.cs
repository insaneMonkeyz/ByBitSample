using Newtonsoft.Json;

namespace MarketDataProvider.Contracts.Bybit.Rest.Market
{
    [Serializable]
    public class OptionSecurityDescription : SecurityDescription
    {
        [JsonProperty("price24hPcnt")]
        public new double? DailyPriceChange { get; set; }

        [JsonProperty("predictedDeliveryPrice")]
        public decimal? ExpectedDeliveryPrice { get; set; }

        [JsonProperty("change24h")]
        public decimal? DailyChange { get; set; }

        [JsonProperty("bid1Iv")]
        public decimal? BidIv { get; set; }

        [JsonProperty("ask1Iv")]
        public decimal? AskIv { get; set; }

        public decimal? MarkIv { get; set; }
        public decimal? MarkPrice { get; set; }
        public decimal? IndexPrice { get; set; }
        public decimal? UnderlyingPrice { get; set; }
        public decimal? TotalVolume { get; set; }
        public decimal? TotalTurnover { get; set; }
        public long? OpenInterest { get; set; }
        public decimal? Delta { get; set; }
        public decimal? Vega { get; set; }
        public decimal? Gamma { get; set; }
        public decimal? Theta { get; set; }
    }
}
