namespace MarketDataProvider
{
    public class Cache<T>
    {
        public TimeSpan UpdatePeriod { get; init; } = TimeSpan.FromHours(6);
        public DateTime LastTimeUpdated { get; private set; }
        public Dictionary<string, T>? Items { get; private set; }

        public bool IsOutdated
        {
            get => DateTime.UtcNow - LastTimeUpdated > UpdatePeriod;
        }

        public void Update(IEnumerable<T> items, Func<T,string> uniqueIdentifierProvider)
        {
            Items = items.ToDictionary(uniqueIdentifierProvider, i=>i);
            LastTimeUpdated = DateTime.UtcNow;
        }
    }
}
