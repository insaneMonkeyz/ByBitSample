using MarketDataProvider.Contracts.Bybit.Stream;
using Newtonsoft.Json;

namespace MarketDataProvider.Contracts
{
    [Serializable]
    public class LinearSecurityUpdate : SecurityUpdate
    {
        [JsonProperty("bid1Price")]
        public decimal? BidPrice { get; set; }

        [JsonProperty("bid1Size")]
        public decimal? BidSize { get; set; }

        [JsonProperty("ask1Price")]
        public decimal? AskPrice { get; set; }

        [JsonProperty("ask1Size")]
        public decimal? AskSize { get; set; }

        [JsonProperty("price24hPcnt")]
        public double? DailyPriceChange { get; set; }

        [JsonProperty("prevPrice24h")]
        public decimal? YesterdayPrice { get; set; }

        [JsonProperty("predictedDeliveryPrice")]
        public decimal? ExpectedDeliveryPrice { get; set; }

        public decimal? MarkPrice { get; set; }
        public long? OpenInterest { get; set; }
        public decimal? OpenInterestValue { get; set; }
        public decimal? FundingRate { get; set; }
        public long? NextFundingTime { get; set; }
        public long? DeliveryTime { get; set; }
        public decimal? BasisRate { get; set; }
        public decimal? DeliveryFreeRate { get; set; }
    }
}