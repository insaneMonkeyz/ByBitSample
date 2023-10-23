using Newtonsoft.Json;

namespace MarketDataProvider.BybitApi.DTO.Stream
{
    [Serializable]
    internal struct TradeDescription
    {
        [JsonProperty("i")]
        public string? Id { get; set; }

        [JsonProperty("T")]
        public long? Timestamp { get; set; }

        [JsonProperty("s")]
        public string? Ticker { get; set; }

        [JsonProperty("S")]
        public string? Side { get; set; }

        [JsonProperty("v")]
        public string? Size { get; set; }

        [JsonProperty("p")]
        public string? Price { get; set; }

        [JsonProperty("L", Required = Required.Default)]
        public string? Direction { get; set; }

        [JsonProperty("BT", Required = Required.Default)]
        public bool? IsBlock { get; set; }
    }

    /*
     
    > T	number	The timestamp (ms) that the order is filled
    > s	string	Symbol name
    > S	string	Side of taker. Buy,Sell
    > v	string	Trade size
    > p	string	Trade price
    > L	string	Direction of price change. Unique field for future
    > i	string	Trade ID
    > BT	boolean	Whether it is a block trade order or not
     
     */
}
