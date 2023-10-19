namespace MarketDataProvider.Exceptions;

public class ConnectionException : Exception
{
    public ConnectionException(string message) : base(message) { }
}
