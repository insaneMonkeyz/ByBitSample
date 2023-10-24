using System.Net.WebSockets;

namespace MarketDataProvider.WebSocket
{
    // ClientWebSocket does not implement any interfaces, which is painful to mock during testing.
    // To avoid messing around with shims and fakes, we'll just wrap the real socket in this class,
    // and make the consumers of the socket depend on the interface
    internal class WebSocketAdapter : IWebSocketClient
    {
        private readonly ClientWebSocket _socket = new();

        public WebSocketState State => _socket.State;

        public async Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
            => await _socket.ConnectAsync(uri, cancellationToken);

        public async Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
            => await _socket.CloseAsync(closeStatus, statusDescription, cancellationToken);

        public async Task SendAsync(
            ReadOnlyMemory<byte> buffer,
                WebSocketMessageType msgType,
                    WebSocketMessageFlags msgFlags,
                        CancellationToken cancellationToken)
        {
            await _socket.SendAsync(buffer, msgType, msgFlags, cancellationToken).AsTask();
        }

        public async Task ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken)
        {
            // The websocket does not automatically clear the buffer before writing to it
            // Each new message overlaps the previous message.
            // And when the new message is shorter than the previous one,
            // the remains of the previous will be present in the result
            buffer.Span.Clear();

            await _socket.ReceiveAsync(buffer, cancellationToken).AsTask();
        }

        public void Abort() => _socket.Abort();

        public void Dispose() => _socket.Dispose();
    }
}