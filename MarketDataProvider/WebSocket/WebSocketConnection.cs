using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using MarketDataProvider.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZeroLog;

namespace MarketDataProvider.WebSocket
{
    internal class WebSocketConnection : IConnectableDataTransmitter
    {
        private static readonly TimeSpan _smallestHeartbeatRepeatFrequency = TimeSpan.FromMilliseconds(100);
        private static readonly TimeSpan _smallestConnectionTimeout = TimeSpan.FromMilliseconds(100);

        private readonly Log _log = LogManager.GetLogger<IConnection>();
        private readonly Log _messageLog = LogManager.GetLogger<IDataTransmitter>();
        private readonly IWebSocketClient _websocket;
        private readonly IHeartbeatProvider _heartbeatMessageFactory;
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
        public event EventHandler<object>? ServerReply;

        public WebSocketConnection(IHeartbeatProvider heartbeatMessageFactory, IAbstractWebSocketFactory socketFactory)
        {
            _heartbeatMessageFactory = heartbeatMessageFactory
                ?? throw new ArgumentNullException(nameof(heartbeatMessageFactory));

            _websocket = socketFactory.CreateWebSocketClient();
        }

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidConfigurationException"></exception>
        /// <exception cref="ConnectionException"></exception>
        public async Task ConnectAsync(ConnectionParameters parameters, CancellationToken userCancellation)
        {
            if (_disposed || ConnectionState == ConnectionState.Connecting
                          || ConnectionState == ConnectionState.Connected)
            {
                _log.Info($"Connection attempt rejected. Socket is already connected");
                return;
            }

            ValidateParameters(parameters);

            _connectionParameters = parameters;
            ConnectionState = ConnectionState.Connecting;

            var infinite = parameters.ReconnectionAttempts == Timeout.Infinite;

            for (int attemptNum = 0; infinite || attemptNum <= parameters.ReconnectionAttempts; attemptNum++)
            {
                using var timeoutCancellation = new CancellationTokenSource(parameters.ConnectionTimeout);
                using var aggregateCancellation = CancellationTokenSource.CreateLinkedTokenSource(userCancellation, timeoutCancellation.Token);

                try
                {
                    if (attemptNum != 0)
                    {
                        await Task.Delay(parameters.ReconnectionInterval, userCancellation);
                        _log.Info($"Reconnect attempt #{attemptNum}");
                    }

                    await _websocket.ConnectAsync(new(parameters.StreamHost!), aggregateCancellation.Token);
                }
                catch (Exception e) when (TaskWasCancelled(e, aggregateCancellation))
                {
                    _log.Error($"Task canceled by system");
                }
                catch (Exception e)
                {
                    _log.Error($"Connection to '{parameters.StreamHost}' failed", e);
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
            throw new ConnectionException($"Could not establish connection with {parameters.StreamHost} even after {parameters.ReconnectionAttempts} attempts");
        }
        public async Task DisconnectAsync(CancellationToken userCancellation)
        {
            if (_disposed || ConnectionState == ConnectionState.Disconnected
                          || ConnectionState == ConnectionState.Disconnecting)
            {
                _log.Warn("Disconnection rejected because the socket is already not connected");
                return;
            }

            _log.Info("Disconnecting");

            ConnectionState = ConnectionState.Disconnecting;

            StopHeartbeatTask();

            using var timeoutCancellation = new CancellationTokenSource(_connectionParameters!.ConnectionTimeout);
            using var aggregateCancellation = CancellationTokenSource.CreateLinkedTokenSource(userCancellation, timeoutCancellation.Token);

            try
            {
                await _websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, aggregateCancellation.Token);
            }
            catch (Exception e) when(TaskWasCancelled(e, aggregateCancellation))
            {
                _log.Error($"Disconnecting was aborted before it has completed", e);
            }
            catch (Exception e)
            {
                _log.Error("Could not properly disconnect the socket", e);
            }

            ConnectionState = ConnectionState.Disconnected;
        }
        public async Task SendDataAsync(object data)
        {
            if (_disposed || ConnectionState != ConnectionState.Connected)
            {
                _log.Warn($"Trying to send data to a closed connection");
                return;
            }

            if (data is null)
            {
                _log.Warn($"Trying to send null data");
                throw new ArgumentNullException(nameof(data));
            }

            try
            {
                var json = JsonConvert.SerializeObject(data);
                _messageLog.Info(json);

                var charMemory = json.AsMemory();
                var buffer = Unsafe.As<ReadOnlyMemory<char>, Memory<byte>>(ref charMemory);

                await _websocket.SendAsync(buffer,
                    WebSocketMessageType.Text,
                        WebSocketMessageFlags.EndOfMessage,
                            CancellationToken.None);
            }
            catch (Exception e)
            {
                _log.Error("Failed to send the data", e);
            }
        }
        public void Dispose()
        {
            if (_disposed)
            {
                _log.Debug($"Trying to dispose already disposed {nameof(WebSocketConnection)}");
                return;
            }

            _log.Debug($"Disposing {nameof(WebSocketConnection)}");

            _disposed = true;
            _websocket?.Dispose();
            _heartbeatTimer?.Dispose();
        }

        private void ValidateParameters(ConnectionParameters parameters)
        {
            if (parameters is null)
            {
                _log.Error($"Trying to use null ConnectionParameter");
                throw new InvalidConfigurationException(nameof(parameters));
            }

            if (parameters.StreamHost is null)
            {
                _log.Error($"Invalid ConnectionParameters. {parameters.StreamHost} is not set");
                throw new InvalidConfigurationException($"{nameof(ConnectionParameters.StreamHost)} is null");
            }

            if (parameters.UseHeartbeating && parameters.HeartbeatInterval < _smallestHeartbeatRepeatFrequency)
            {
                _log.Error($"Invalid ConnectionParameters. Heartbeat frequency is too high");

                throw new ArgumentException(string.Format(
                    "If {0}.{1} is set, {0}.{2} must be at least {3}. Setting the value too small will cause " +
                    "the server to get flooded, resulting in the connection being aborted.",
                /*0*/   nameof(ConnectionParameters),
                    /*1*/   nameof(ConnectionParameters.UseHeartbeating),
                        /*2*/   nameof(ConnectionParameters.HeartbeatInterval),
                            /*3*/   _smallestHeartbeatRepeatFrequency));
            }

            if (parameters.ConnectionTimeout < _smallestConnectionTimeout &&
                parameters.ConnectionTimeout != Timeout.InfiniteTimeSpan)
            {
                _log.Error($"Invalid ConnectionParameters. Connection timeout is too small");

                throw new ArgumentException(
                    $"A minimum connection timeout of {_smallestConnectionTimeout} is allowed");
            }
        }
        private void BeginListening()
        {
            Task.Run(() =>
            {
                var buffer = new byte[1024];

                _log.Info($"Begin listening to the server");

                while (!_disposed && _websocket.State == WebSocketState.Open)
                {
                    using var timeoutCancellation = new CancellationTokenSource(_connectionParameters!.ConnectionTimeout);

                    try
                    {
                        _websocket.ReceiveAsync(buffer, timeoutCancellation.Token).Wait();

                        var json = Encoding.UTF8.GetString(buffer);

                        _messageLog.Info(json);

                        var message = JObject.Parse(json);

                        if (!_heartbeatMessageFactory.IsHeartbeatReply(message))
                        {
                            ServerReply?.Invoke(this, message); 
                        }
                    }
                    catch(WebSocketException e)
                    {
                        _log.Error($"Socket faulted. Requesting reconnection", e);
                        ConnectAsync(_connectionParameters, CancellationToken.None);
                    }
                    catch (Exception e) when (TaskWasCancelled(e, timeoutCancellation))
                    {
                        _log.Error($"Server did not reply in {_connectionParameters.ConnectionTimeout}", e);
                        DisconnectAsync(CancellationToken.None);
                    }
                    catch (Exception e)
                    {
                        _log.Error($"Reception from the server failed", e);
                    }
                }
            });
        }
        private bool TaskWasCancelled(Exception e, CancellationTokenSource cancellation)
        {
            return (e is TaskCanceledException ||
                    e is AggregateException ae && ae.InnerException is TaskCanceledException)
                    && cancellation.IsCancellationRequested
                    && _websocket.State == WebSocketState.Open;
        }
        private void SetHeartbeatTask(TimeSpan period)
        {
            if (_connectionParameters!.UseHeartbeating is false)
            {
                _log.Debug($"Heartbeating is disabled and will not be set");
                return;
            }

            if (_heartbeatTimer is not null)
            {
                _log.Debug($"Heartbeating period changed to {period}");
                _heartbeatTimer.Change(period, period);
                return;
            }

            _log.Debug($"Heartbeating initiated");
            _heartbeatTimer = new Timer(OnHeartbeatRequired, null, period, period);
        }
        private void StopHeartbeatTask()
        {
            _heartbeatTimer?.Dispose();
            _heartbeatTimer = null;
            _log.Debug($"Heartbeating stopped");
        }
        private void OnHeartbeatRequired(object? _)
        {
            if (ConnectionState != ConnectionState.Connected)
            {
                return;
            }

            var heartbeatObj = _heartbeatMessageFactory.GetNextMessage();
            SendDataAsync(heartbeatObj);
        }
    }
}
