using Newtonsoft.Json;

namespace MarketDataProvider.BybitApi.DTO.Rest
{
    [Serializable]
    public struct ResponseData<T>
    {
        public string? Category { get; set; }

        [JsonProperty("list")]
        public T[]? Entities { get; set; }
    }
}
