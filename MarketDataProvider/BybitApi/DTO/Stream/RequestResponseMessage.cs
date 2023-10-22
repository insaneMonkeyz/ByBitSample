using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace MarketDataProvider.BybitApi.DTO.Stream
{
    [Serializable]
    public class RequestResponseMessage
    {
        [JsonProperty("req_id")]
        public string? Id { get; set; }

        [JsonProperty("op")]
        public string? Operation { get; set; }

        [JsonProperty("conn_id")]
        public string? ConnectionId { get; set; }

        [JsonProperty("ret_msg")]
        public string? Message { get; set; }

        [JsonProperty("success")]
        public bool? Success { get; set; }
    }
}
