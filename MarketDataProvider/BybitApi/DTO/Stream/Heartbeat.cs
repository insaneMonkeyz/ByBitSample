using System.Text.Json.Serialization;

namespace MarketDataProvider.BybitApi.DTO.Stream
{
    [Serializable]
    public class Heartbeat
    {
        public const string RequestMessage = "ping";
        public const string ReplyMessage = "pong";

        [JsonPropertyName("req_id")]
        public int Id { get; set; }

        [JsonPropertyName("op")]
        public string? Operation { get; set; } = RequestMessage;
    }
}
