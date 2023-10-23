using System.Runtime.Serialization;

namespace MarketDataProvider
{
    [Serializable]
    public enum Sides
    {
        Unknown,
        [EnumMember(Value = "Buy")]
        Buy,
        [EnumMember(Value = "Sell")]
        Sell
    }
}
