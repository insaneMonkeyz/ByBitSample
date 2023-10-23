using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace MarketDataProvider.BybitApi.DTO.Stream
{
    [Serializable]
    public struct RequestMessage
    {
        [JsonProperty("req_id")]
        [JsonPropertyName("req_id")]
        public string? Id { get; set; }

        [JsonProperty("op")]
        [JsonPropertyName("op")]
        public string? Operation { get; set; }

        public object[]? Args { get; set; }
    }
}
