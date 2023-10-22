namespace MarketDataProvider.BybitApi
{
    internal static class StreamOperations
    {
        public const string Heartbeat = "ping";
        public const string HeartbeatReply = "pong";

        public const string Subscribe = "subscribe";
        public const string Unsubscribe = "unsubscribe";
    }
}
