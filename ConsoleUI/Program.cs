using System.Xml.Serialization;
using MarketDataProvider;
using MarketDataProvider.Bybit.Rest;

namespace ConsoleUI;

internal partial class Program
{
    private static async Task Main(string[] args)
    {
        BuildApp();
        ConsoleUI.Initialize(new());
        ConsoleUI.PromptUser("Input something for test please: ");
        return;
    }

    private static async Task Test()
    {
        var connection = ConnectionsFactory.CreateByBitConnection();
        var provider = new BybitMarketDataProvider(connection);

        var connectionParameters = new ConnectionParameters()
        {
            ConnectionTimeout = TimeSpan.FromSeconds(30),
            StreamHost = "wss://stream.bybit.com/v5/public/spot",
        };

        await provider.ConnectAsync(connectionParameters, CancellationToken.None);

        var filter = new Filter()
        {
            Kind = SecurityKind.Spot,
            TickerTemplate = "BTCUSDT"
        };

        var securities = await provider.GetAvailablSecuritiesAsync(filter);

        var btc = securities.First();

        provider.NewTrades += (_, t) => Console.WriteLine($"{t.Timestamp} {t.Security.Ticker} {t.Side} {t.Price} {t.Size}");

        await provider.SubscribeTradesAsync(btc);

        await Task.Delay(TimeSpan.FromSeconds(30));
    }

    private static void BuildApp()
    {
    }
}