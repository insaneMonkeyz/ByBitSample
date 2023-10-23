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
        await Task.Delay(Timeout.Infinite);
    }

    private static void BuildApp()
    {
    }
}