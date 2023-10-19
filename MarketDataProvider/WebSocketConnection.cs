using System.Net.WebSockets;
using System.Text.Json;
using MarketDataProvider.WebSocket;
using Microsoft.Extensions.Logging;

namespace MarketDataProvider
{
    internal class WebSocketConnection : IConnection, IDisposable
    {
        private readonly ILogger _log;
        private readonly IWebSocketClient _websocket;
        private readonly IHeartbeatMessageFactory _heartbeatMessageFactory;
        private ConnectionParameters? _connectionParameters;
        private Timer? _heartbeatTimer;
        private bool _disposed;

        private ConnectionState _connectionState;
        public ConnectionState ConnectionState
        {
            get => _connectionState;
            set
            {
                if (value == _connectionState)
                {
                    return;
                }

                _connectionState = value;

                ConnectionStateChanged?.Invoke(this, value);
            }
        }

        public event EventHandler<ConnectionState>? ConnectionStateChanged;

        public WebSocketConnection(ILogger log, IHeartbeatMessageFactory heartbeatMessageFactory, IAbstractWebSocketFactory socketFactory)
        {
            _heartbeatMessageFactory = heartbeatMessageFactory 
                ?? throw new ArgumentNullException(nameof(heartbeatMessageFactory));

            _log = log;// ?? throw new ArgumentNullException(nameof(log));

            _websocket = socketFactory.CreateWebSocketClient();
        }

        public async Task ConnectAsync(ConnectionParameters parameters, CancellationToken userCancellation)
        {
            if (_disposed || ConnectionState == ConnectionState.Connecting
                          || ConnectionState == ConnectionState.Connected)
            {
                return;
            }

            _connectionParameters = parameters ?? throw new ArgumentNullException(nameof(parameters));

            ConnectionState = ConnectionState.Connecting;

            for (int attemptNum = 0; attemptNum <= parameters.ReconnectionAttempts; attemptNum++)
            {
                if (attemptNum != 0)
                {
                    await Task.Delay(parameters.ReconnectionInterval, userCancellation);
                }

                using var timeoutCancellation = new CancellationTokenSource(parameters.ConnectionTimeout);
                using var aggregateCancellation = CancellationTokenSource.CreateLinkedTokenSource(userCancellation, timeoutCancellation.Token);

                await _websocket.ConnectAsync(new(parameters.Uri), aggregateCancellation.Token);

                if (_websocket.State == WebSocketState.Open)
                {
                    ConnectionState = ConnectionState.Connected;
                    SetHeartbeatTask(parameters.HeartbeatInterval);
                    return;
                }
            }

            throw new Exception($"Could not establish connection with {parameters.Uri} even after {parameters.ReconnectionAttempts} attempts");
        }
        public async Task DisconnectAsync(CancellationToken userCancellation)
        {
            if (_disposed || ConnectionState == ConnectionState.Disconnected
                          || ConnectionState == ConnectionState.Disconnecting)
            {
                return;
            }

            ConnectionState = ConnectionState.Disconnecting;

            StopHeartbeatTask();

            using var timeoutCancellation = new CancellationTokenSource(_connectionParameters!.ConnectionTimeout);
            using var aggregateCancellation = CancellationTokenSource.CreateLinkedTokenSource(userCancellation, timeoutCancellation.Token);

            await _websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, aggregateCancellation.Token);

            ConnectionState = ConnectionState.Disconnected;
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
                var msgBuffer = JsonSerializer.SerializeToUtf8Bytes(heartbeatObj);

                var faulted =
                    _websocket
                        .SendAsync(msgBuffer, WebSocketMessageType.Text, WebSocketMessageFlags.None, CancellationToken.None)
                        .IsFaulted;

                if (faulted)
                {
                    throw new Exception("WebSocket did not send the message");
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, "Failed to send the heartbeat");
            }
        }
    }
}
