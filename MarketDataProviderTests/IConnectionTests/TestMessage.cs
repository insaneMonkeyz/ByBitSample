namespace MarketDataProviderTests.IConnectionTests
{
    [Serializable]
    public class TestMessage
    {
        public int IntegerProperty { get; set; }
        public string? StringProperty { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is TestMessage another
                && another.IntegerProperty == IntegerProperty
                && another.StringProperty == StringProperty;
        }
        public override int GetHashCode()
        {            
            return HashCode.Combine(IntegerProperty, StringProperty);
        }
    }
}