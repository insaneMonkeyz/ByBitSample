using MarketDataProvider.BybitApi.DTO.Stream;

namespace MarketDataProvider
{
    internal static class MessageToEntityConverter
    {
        public static Trade? Convert(StreamMessage<TradeDescription>? message, Func<string,ISecurity?> resolveSecurity)
        {
            if (message?.Data == null)
            {
                return default;
            }

            var trade = message.Value.Data.FirstOrDefault();

            if (string.IsNullOrEmpty(trade.Ticker))
            {
                return default;
            }

            if (resolveSecurity(trade.Ticker) is not Security sec)
            {
                return default;
            }

            try
            {
                return new Trade
                {
                    Security = sec,
                    Price = decimal.Parse(trade.Price),
                    Size = decimal.Parse(trade.Size),
                    Side = Enum.Parse<Sides>(trade.Side),
                    Timestamp = trade.Timestamp.Value.ToBybitTimestamp(),
                };
            }
            catch
            {
                return default;
            }
        }
    }
}
