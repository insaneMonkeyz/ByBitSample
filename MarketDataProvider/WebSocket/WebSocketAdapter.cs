﻿using System.Net.WebSockets;

namespace MarketDataProvider.WebSocket
{
    // ClientWebSocket does not implement any interfaces, which is painful to mock during testing.
    // To avoid messing around with shims and fakes, we'll just wrap the real socket in this class,
    // and make the consumers of the socket depend on the interface
    public class WebSocketAdapter : IWebSocketClient
    {
        private readonly ClientWebSocket _socket = new();

        public WebSocketState State => _socket.State;

        public async Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
            => await _socket.ConnectAsync(uri, cancellationToken);

        public async Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
            => await _socket.CloseAsync(closeStatus, statusDescription, cancellationToken);

        public ValueTask SendAsync(
            ReadOnlyMemory<byte> buffer,
                WebSocketMessageType msgType,
                    WebSocketMessageFlags msgFlags,
                        CancellationToken cancellationToken)
        {
            return _socket.SendAsync(buffer, msgType, msgFlags, cancellationToken);
        }

        public ValueTask<ValueWebSocketReceiveResult> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken)
            => _socket.ReceiveAsync(buffer, cancellationToken);

        public void Abort() => _socket.Abort();

        public void Dispose() => _socket.Dispose();
    }
}