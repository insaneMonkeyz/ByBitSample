using Newtonsoft.Json;

namespace MarketDataProvider.BybitApi.DTO.Stream
{
    [Serializable]
    public struct StreamMessage<T>
    {
        [JsonProperty("id", Required = Required.Default)]
        public string? Id { get; set; }

        [JsonProperty("topic")]
        public string? Topic { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("ts")]
        public long? Timestamp { get; set; }

        [JsonProperty("cs", Required = Required.Default)]
        public long? CrossSequence { get; set; }

        [JsonProperty("data", Required = Required.Default)]
        public T[]? Data { get; set; }
    }
}