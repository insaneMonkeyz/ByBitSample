using Newtonsoft.Json;

namespace MarketDataProvider.Contracts.Bybit.Rest
{
    [Serializable]
    public class ResponseData<T>
    {
        public string? Category { get; set; }

        [JsonProperty("list")]
        public T[]? Entities { get; set; }
    }
}
