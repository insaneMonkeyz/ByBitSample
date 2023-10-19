using System.Net.WebSockets;

namespace MarketDataProvider.WebSocket
{
    public interface IWebSocketClient : IDisposable
    {
        WebSocketState State { get; }

        Task ConnectAsync(Uri uri, CancellationToken cancellationToken);
        Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken);

        ValueTask<ValueWebSocketReceiveResult> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken);

        ValueTask SendAsync(
            ReadOnlyMemory<byte> buffer,
                WebSocketMessageType msgType,
                    WebSocketMessageFlags msgFlags,
                        CancellationToken cancellationToken);

        void Abort();
    }
}