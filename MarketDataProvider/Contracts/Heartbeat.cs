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
        [JsonPropertyName("req_id")]
        public int Id { get; set; }

        [JsonPropertyName("op")]
        public string? Operation => "ping";
    }
}
