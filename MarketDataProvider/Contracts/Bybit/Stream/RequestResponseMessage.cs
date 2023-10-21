using System.Text.Json.Serialization;

namespace MarketDataProvider.Contracts.Bybit.Stream
{
    [Serializable]
    public class RequestResponseMessage
    {
        [JsonPropertyName("req_id")]
        public int? Id { get; set; }

        [JsonPropertyName("op")]
        public string? Operation { get; set; }

        [JsonPropertyName("conn_id")]
        public string? ConnectionId { get; set; }

        [JsonPropertyName("ret_msg")]
        public string? Message { get; set; }

        public bool? Success { get; set; }

        public object[]? Args { get; set; }
    }
}
