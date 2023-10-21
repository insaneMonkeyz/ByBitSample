using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MarketDataProvider.Contracts.ByBit
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
