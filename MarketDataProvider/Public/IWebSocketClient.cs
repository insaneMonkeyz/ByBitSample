using System.Net.WebSockets;

namespace MarketDataProvider.WebSocket
{
    public interface IWebSocketClient : IDisposable
    {
        WebSocketState State { get; }

        Task ConnectAsync(Uri uri, CancellationToken cancellationToken);
        Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken);

        Task ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken);

        Task SendAsync(
            ReadOnlyMemory<byte> buffer,
                WebSocketMessageType msgType,
                    WebSocketMessageFlags msgFlags,
                        CancellationToken cancellationToken);

        void Abort();
    }
}