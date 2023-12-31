﻿using Newtonsoft.Json;

namespace MarketDataProvider.BybitApi.DTO.Rest
{
    [Serializable]
    public struct Response<T>
    {
        [JsonProperty("retCode")]
        public int ReturnCode { get; set; }

        [JsonProperty("retMsg")]
        public string? Description { get; set; }

        [JsonProperty("time")]
        public long Timestamp { get; set; }

        [JsonProperty("result")]
        public ResponseData<T>? Data { get; set; }

        [JsonProperty("retExtInfo")]
        public object? ExtraData { get; set; }
    }
}