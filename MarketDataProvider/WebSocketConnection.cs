using System.Net.WebSockets;
using Microsoft.Extensions.Logging;

namespace MarketDataProvider
{
    internal class WebSocketConnection : IConnection, IDisposable
    {
        private readonly ILogger _log;
        private readonly ClientWebSocket _websocket;
        private readonly IHeartbeatMessageFactory _heartbeatMessageFactory;
        private ConnectionParameters? _connectionParameters;
        private Timer? _heartbeatTimer;
        private bool _disposed;

        public ConnectionState ConnectionState
        {
            get => _websocket.State switch
            {
                WebSocketState.Open => ConnectionState.Connected,
                WebSocketState.Connecting => ConnectionState.Connecting,
                WebSocketState.CloseSent => ConnectionState.Disconnecting,
                _ => ConnectionState.Disconnected
            };
        }

        public event EventHandler<ConnectionState>? ConnectionStateChanged;

        public WebSocketConnection(ILogger? log, IHeartbeatMessageFactory? heartbeatMessageFactory)
        {
            _heartbeatMessageFactory = heartbeatMessageFactory 
                ?? throw new ArgumentNullException(nameof(heartbeatMessageFactory));

            _log = log ?? throw new ArgumentNullException(nameof(log));

            _websocket = new ClientWebSocket();
        }

        public async Task ConnectAsync(ConnectionParameters parameters, CancellationToken userCancellation)
        {
            if (_disposed)
            {
                return;
            }

            _connectionParameters = parameters ?? throw new ArgumentNullException(nameof(parameters));

            for (int attemptNum = 0; attemptNum <= parameters.ReconnectionAttempts; attemptNum++)
            {
                if (attemptNum != 0)
                {
                    await Task.Delay(parameters.ReconnectionInterval, userCancellation);
                }

                using var timeoutCancellation = new CancellationTokenSource(parameters.ConnectionTimeout);
                using var aggregateCancellation = CancellationTokenSource.CreateLinkedTokenSource(userCancellation, timeoutCancellation.Token);

                await _websocket.ConnectAsync(parameters.Uri, aggregateCancellation.Token);

                if (_websocket.State == WebSocketState.Open)
                {
                    SetHeartbeatTask(parameters.HeartbeatInterval);
                    return;
                }
            }

            throw new Exception($"Could not establish connection with {parameters.Uri} even after {parameters.ReconnectionAttempts} attempts");
        }
        public async Task DisconnectAsync(CancellationToken userCancellation)
        {
            if (_disposed || _websocket.State != WebSocketState.Open)
            {
                return;
            }

            StopHeartbeatTask();

            using var timeoutCancellation = new CancellationTokenSource(_connectionParameters!.ConnectionTimeout);
            using var aggregateCancellation = CancellationTokenSource.CreateLinkedTokenSource(userCancellation, timeoutCancellation.Token);

            await _websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, aggregateCancellation.Token);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _websocket?.Dispose();
            _heartbeatTimer?.Dispose();
        }

        private void SetHeartbeatTask(TimeSpan period)
        {
            if (_connectionParameters?.UseHeartbeating is not true)
            {
                return;
            }

            if (_heartbeatTimer is not null)
            {
                _heartbeatTimer.Change(period, period);
                return;
            }

            _heartbeatTimer = new Timer(OnHeartbeatRequired, null, period, period);
        }
        private void StopHeartbeatTask()
        {
            _heartbeatTimer?.Dispose();
            _heartbeatTimer = null;
        }
        private void OnHeartbeatRequired(object? arg)
        {
            if (ConnectionState != ConnectionState.Connected)
            {
                return;
            }

            try
            {
                var heartbeatObj = _heartbeatMessageFactory.GetNextMessage();
                var msgBuffer = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(heartbeatObj);

                _websocket
                    .SendAsync(msgBuffer, WebSocketMessageType.Text, false, CancellationToken.None)
                    .Wait(_connectionParameters!.ConnectionTimeout);
            }
            catch (Exception e)
            {
                _log.LogError(e, "Failed to send the heartbeat");
            }
        }
    }
}
