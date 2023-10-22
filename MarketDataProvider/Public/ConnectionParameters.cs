using System.Runtime.Serialization;

namespace MarketDataProvider
{
    [Serializable]
    public class ConnectionParameters
    {
        public string? Uri { get; set; }
        public int ReconnectionAttempts { get; set; }
        public TimeSpan ReconnectionInterval { get; set; }
        public TimeSpan ConnectionTimeout { get; set; }
        public bool UseHeartbeating { get; set; }
        public TimeSpan HeartbeatInterval { get; set; }
    }
}
