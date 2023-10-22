using Newtonsoft.Json;

namespace MarketDataProvider.BybitApi.DTO.Stream
{
    [Serializable]
    public class StreamMessage<T>
    {
        public string? Topic { get; set; }
        public string? Type { get; set; }

        [JsonProperty("ts")]
        public long? Timestamp { get; set; }

        [JsonProperty("cs")]
        public long? CrossSequence { get; set; }

        public T? Data { get; set; }
    }
}