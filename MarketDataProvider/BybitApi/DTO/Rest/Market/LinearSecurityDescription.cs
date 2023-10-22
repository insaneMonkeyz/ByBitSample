using Newtonsoft.Json;

namespace MarketDataProvider.BybitApi.DTO.Rest.Market
{
    [Serializable]
    public class LinearSecurityDescription : SecurityDescription
    {
        [JsonProperty("prevPrice1h")]
        public decimal? OneHourBackPrice { get; set; }

        [JsonProperty("predictedDeliveryPrice")]
        public decimal? ExpectedDeliveryPrice { get; set; }

        public decimal? MarkPrice { get; set; }
        public decimal? IndexPrice { get; set; }
        public long? OpenInterest { get; set; }
        public decimal? OpenInterestValue { get; set; }
        public decimal? FundingRate { get; set; }
        public long? NextFundingTime { get; set; }
        public long? DeliveryTime { get; set; }
        public decimal? BasisRate { get; set; }
        public decimal? DeliveryFreeRate { get; set; }
    }
}