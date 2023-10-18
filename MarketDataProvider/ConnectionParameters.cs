using System.Runtime.Serialization;

namespace MarketDataProvider
{
    public class ConnectionParameters
    {
        public required Uri Uri { get; set; }
        public int ReconnectionAttempts { get; set; }
        public TimeSpan ReconnectionInterval { get; set; }
        public TimeSpan ConnectionTimeout { get; set; }
        public bool UseHeartbeating { get; set; }
        public TimeSpan HeartbeatInterval { get; set; }
    }
}
