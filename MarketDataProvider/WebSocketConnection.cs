using System.Net.WebSockets;
using System.Text.Json;
using MarketDataProvider.Exceptions;
using MarketDataProvider.WebSocket;
using Microsoft.Extensions.Logging;

namespace MarketDataProvider
{
    internal class WebSocketConnection : IConnection
    {
        private static readonly TimeSpan _smallestHeartbeatRepeatFrequency = TimeSpan.FromMilliseconds(100);
        private static readonly TimeSpan _smallestConnectionTimeout = TimeSpan.FromMilliseconds(100);

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

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ConnectionException"></exception>
        public async Task ConnectAsync(ConnectionParameters parameters, CancellationToken userCancellation)
        {
            if (_disposed || ConnectionState == ConnectionState.Connecting
                          || ConnectionState == ConnectionState.Connected)
            {
                return;
            }

            ValidateParameters(parameters);

            _connectionParameters = parameters;
            ConnectionState = ConnectionState.Connecting;

            var infinite = parameters.ReconnectionAttempts == Timeout.Infinite;

            for (int attemptNum = 0; infinite || attemptNum <= parameters.ReconnectionAttempts; attemptNum++)
            {
                if (attemptNum != 0)
                {
                    await Task.Delay(parameters.ReconnectionInterval, userCancellation);
                }

                using var timeoutCancellation = new CancellationTokenSource(parameters.ConnectionTimeout);
                using var aggregateCancellation = CancellationTokenSource.CreateLinkedTokenSource(userCancellation, timeoutCancellation.Token);

                try
                {
                    await _websocket.ConnectAsync(new(parameters.Uri!), aggregateCancellation.Token);
                }
                catch (Exception)
                {
                    // TODO: log exception
                    continue;
                }

                if (_websocket.State == WebSocketState.Open)
                {
                    ConnectionState = ConnectionState.Connected;
                    BeginListening();
                    SetHeartbeatTask(parameters.HeartbeatInterval);
                    return;
                }
            }

            ConnectionState = ConnectionState.Disconnected;
            throw new ConnectionException($"Could not establish connection with {parameters.Uri} even after {parameters.ReconnectionAttempts} attempts");
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

        private void ValidateParameters(ConnectionParameters parameters)
        {
            if (parameters is null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (parameters.Uri is null)
            {
                throw new ArgumentException($"uri is null");
            }

            if (parameters.UseHeartbeating && parameters.HeartbeatInterval < _smallestHeartbeatRepeatFrequency)
            {
                throw new ArgumentException(string.Format(
                    "If {0}.{1} is set, {0}.{2} must be at least {3}. Setting the value too small will cause " +
                    "the server to flood, resulting in the connection being aborted.",
                /*0*/   nameof(ConnectionParameters),
                    /*1*/   nameof(ConnectionParameters.UseHeartbeating),
                        /*2*/   nameof(ConnectionParameters.HeartbeatInterval),
                            /*3*/   _smallestHeartbeatRepeatFrequency)); 
            }

            if (parameters.ConnectionTimeout < _smallestConnectionTimeout &&
                parameters.ConnectionTimeout != Timeout.InfiniteTimeSpan)
            {
                throw new ArgumentException(
                    $"A minimum connection timeout of {_smallestConnectionTimeout} is allowed");
            }
        }

        private void BeginListening()
        {
            Task.Run(() =>
            {
                var buffer = new byte[1024];

                while (!_disposed && _websocket.State == WebSocketState.Open)
                {
                    using var timeoutCancellation = new CancellationTokenSource(_connectionParameters!.ConnectionTimeout);

                    try
                    {
                        _websocket.ReceiveAsync(buffer, timeoutCancellation.Token).Wait();
                    }
                    catch (Exception) when (timeoutCancellation.IsCancellationRequested && _websocket.State == WebSocketState.Open)
                    {
                        DisconnectAsync(CancellationToken.None);
                    }
                }
            });
        }

        private void SetHeartbeatTask(TimeSpan period)
        {
            if (_connectionParameters!.UseHeartbeating is false)
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

                _websocket
                    .SendAsync(msgBuffer, WebSocketMessageType.Text, WebSocketMessageFlags.None, CancellationToken.None)
                        .Wait();
            }
            catch (Exception e)
            {
                _log.LogError(e, "Failed to send the heartbeat");
            }
        }
    }
}
