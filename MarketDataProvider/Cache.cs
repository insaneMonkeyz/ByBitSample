namespace MarketDataProvider
{
    public class Cache<T>
    {
        public TimeSpan UpdatePeriod { get; init; } = TimeSpan.FromHours(6);
        public DateTime LastTimeUpdated { get; }
        public HashSet<T>? Items { get; private set; }

        public bool IsOutdated
        {
            get => DateTime.UtcNow - LastTimeUpdated > UpdatePeriod;
        }

        public void Update(IEnumerable<T> items)
        {
            Items = items.ToHashSet();
        }
    }
}
