using System.Text.Json.Serialization;

namespace MarketDataProvider.Contracts.Bybit.Stream
{
    [Serializable]
    public class RequestMessage
    {
        [JsonPropertyName("req_id")]
        public int Id { get; set; }

        [JsonPropertyName("op")]
        public string? Operation { get; set; }

        public object[]? Args { get; set; }
    }
}
