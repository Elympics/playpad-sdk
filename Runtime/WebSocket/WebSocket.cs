/*
 * unity-websocket-webgl
 *
 * @author Jiri Hybek <jiri@hybek.cz>
 * @copyright 2018 Jiri Hybek <jiri@hybek.cz>
 * @license Apache 2.0 - See LICENSE file distributed with this source code.
 */

/*
 * With modifications licensed to Elympics Sp. z o.o.
 */

using System;
using System.Collections.Generic;
#if !UNITY_WEBGL || UNITY_EDITOR
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
#endif

namespace ElympicsPlayPad.WebSockets
{
    /// <summary>
    /// Handler for WebSocket Open event.
    /// </summary>
    public delegate void WebSocketOpenEventHandler();

    /// <summary>
    /// Handler for message received from WebSocket.
    /// </summary>
    public delegate void WebSocketMessageEventHandler(byte[] data);

    /// <summary>
    /// Handler for an error event received from WebSocket.
    /// </summary>
    public delegate void WebSocketErrorEventHandler(string errorMsg);

    /// <summary>
    /// Handler for WebSocket Close event.
    /// </summary>
    public delegate void WebSocketCloseEventHandler(WebSocketCloseCode closeCode, string reason);

    /// <summary>
    /// Enum representing WebSocket connection state
    /// </summary>
    public enum WebSocketState
    {
        Connecting,
        Open,
        Closing,
        Closed
    }

    /// <summary>
    /// Web socket close codes.
    /// </summary>
    public enum WebSocketCloseCode
    {
        /* Do NOT use NotSet - it's only purpose is to indicate that the close code cannot be parsed. */
        NotSet = 0,
        Normal = 1000,
        Away = 1001,
        ProtocolError = 1002,
        UnsupportedData = 1003,
        Undefined = 1004,
        NoStatus = 1005,
        Abnormal = 1006,
        InvalidData = 1007,
        PolicyViolation = 1008,
        TooBig = 1009,
        MandatoryExtension = 1010,
        ServerError = 1011,
        TlsHandshakeFailure = 1015
    }

    /// <summary>
    /// WebSocket class interface shared by both native and JSLIB implementation.
    /// </summary>
    public interface IWebSocket
    {
        /// <summary>
        /// Open WebSocket connection
        /// </summary>
        void Connect();

        /// <summary>
        /// Close WebSocket connection with optional status code and reason.
        /// </summary>
        /// <param name="code">Close status code.</param>
        /// <param name="reason">Reason string.</param>
        void Close(WebSocketCloseCode code = WebSocketCloseCode.Normal, string reason = null);

        /// <summary>
        /// Send binary data over the socket.
        /// </summary>
        /// <param name="data">Payload data.</param>
        void Send(byte[] data);

        /// <summary>
        /// Return WebSocket connection state.
        /// </summary>
        /// <returns>The state.</returns>
        WebSocketState GetState();

        /// <summary>
        /// Occurs when the connection is opened.
        /// </summary>
        event WebSocketOpenEventHandler OnOpen;

        /// <summary>
        /// Occurs when a message is received.
        /// </summary>
        event WebSocketMessageEventHandler OnMessage;

        /// <summary>
        /// Occurs when an error was reported from WebSocket.
        /// </summary>
        event WebSocketErrorEventHandler OnError;

        /// <summary>
        /// Occurs when the socked was closed.
        /// </summary>
        event WebSocketCloseEventHandler OnClose;
    }

    /// <summary>
    /// Various helpers to work mainly with enums and exceptions.
    /// </summary>
    public static class WebSocketHelpers
    {
        /// <summary>
        /// Safely parse close code enum from int value.
        /// </summary>
        /// <returns>The close code enum.</returns>
        /// <param name="closeCode">Close code as int.</param>
        public static WebSocketCloseCode ParseCloseCodeEnum(int closeCode)
        {
            if (Enum.IsDefined(typeof(WebSocketCloseCode), closeCode))
                return (WebSocketCloseCode)closeCode;
            return WebSocketCloseCode.Undefined;
        }

        /*
         * Return error message based on int code
         *

         */
        /// <summary>
        /// Return an exception instance based on int code.
        ///
        /// Used for resolving JSLIB errors to meaninfull messages.
        /// </summary>
        /// <returns>Instance of an exception.</returns>
        /// <param name="errorCode">Error code.</param>
        /// <param name="inner">Inner exception</param>
        public static WebSocketException GetErrorMessageFromCode(int errorCode, Exception inner)
        {
            return errorCode switch
            {
                -1 => new WebSocketUnexpectedException("WebSocket instance not found.", inner),
                -2 => new WebSocketInvalidStateException("WebSocket is already connected or in connecting state.", inner),
                -3 => new WebSocketInvalidStateException("WebSocket is not connected.", inner),
                -4 => new WebSocketInvalidStateException("WebSocket is already closing.", inner),
                -5 => new WebSocketInvalidStateException("WebSocket is already closed.", inner),
                -6 => new WebSocketInvalidStateException("WebSocket is not in open state.", inner),
                -7 => new WebSocketInvalidArgumentException("Cannot close WebSocket. An invalid code was specified or reason is too long.", inner),
                _ => new WebSocketUnexpectedException("Unknown error.", inner),
            };
        }
    }

    /// <summary>
    /// Generic WebSocket exception class
    /// </summary>
    public class WebSocketException : Exception
    {
        public WebSocketException()
        { }

        public WebSocketException(string message)
            : base(message)
        { }

        public WebSocketException(string message, Exception inner)
            : base(message, inner)
        { }
    }

    /// <summary>
    /// Web socket exception raised when an error was not expected, probably due to corrupted internal state.
    /// </summary>
    public class WebSocketUnexpectedException : WebSocketException
    {
        public WebSocketUnexpectedException() { }
        public WebSocketUnexpectedException(string message) : base(message) { }
        public WebSocketUnexpectedException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Invalid argument exception raised when bad arguments are passed to a method.
    /// </summary>
    public class WebSocketInvalidArgumentException : WebSocketException
    {
        public WebSocketInvalidArgumentException() { }
        public WebSocketInvalidArgumentException(string message) : base(message) { }
        public WebSocketInvalidArgumentException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Invalid state exception raised when trying to invoke action which cannot be done due to different then required state.
    /// </summary>
    public class WebSocketInvalidStateException : WebSocketException
    {
        public WebSocketInvalidStateException() { }
        public WebSocketInvalidStateException(string message) : base(message) { }
        public WebSocketInvalidStateException(string message, Exception inner) : base(message, inner) { }
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    /// <summary>
    /// WebSocket class bound to JSLIB.
    /// </summary>
    public class WebSocket : IWebSocket
    {
        /* WebSocket JSLIB functions */
        [DllImport("__Internal")]
        public static extern int ElympicsWebSocketConnect(int instanceId);

        [DllImport("__Internal")]
        public static extern int ElympicsWebSocketClose(int instanceId, int code, string reason);

        [DllImport("__Internal")]
        public static extern int ElympicsWebSocketSend(int instanceId, byte[] dataPtr, int dataLength);

        [DllImport("__Internal")]
        public static extern int ElympicsWebSocketGetState(int instanceId);

        /// <summary>
        /// The instance identifier.
        /// </summary>
        protected int instanceId;

        /// <summary>
        /// Occurs when the connection is opened.
        /// </summary>
        public event WebSocketOpenEventHandler OnOpen;

        /// <summary>
        /// Occurs when a message is received.
        /// </summary>
        public event WebSocketMessageEventHandler OnMessage;

        /// <summary>
        /// Occurs when an error was reported from WebSocket.
        /// </summary>
        public event WebSocketErrorEventHandler OnError;

        /// <summary>
        /// Occurs when the socked was closed.
        /// </summary>
        public event WebSocketCloseEventHandler OnClose;

        /// <summary>
        /// Constructor - receive JSLIB instance id of allocated socket
        /// </summary>
        /// <param name="instanceId">Instance identifier.</param>
        internal WebSocket(int instanceId)
        {
            this.instanceId = instanceId;
        }

        /// <summary>
        /// Destructor - notifies WebSocketFactory about it to remove JSLIB references
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="T:global::ElympicsPlayPad.WebSockets.WebSocket"/> is reclaimed by garbage collection.
        /// </summary>
        ~WebSocket()
        {
            WebSocketFactory.HandleInstanceDestroy(this.instanceId);
        }

        /// <summary>
        /// Return JSLIB instance ID
        /// </summary>
        /// <returns>The instance identifier.</returns>
        public int GetInstanceId()
        {
            return this.instanceId;
        }

        /// <summary>
        /// Open WebSocket connection
        /// </summary>
        public void Connect()
        {
            int ret = ElympicsWebSocketConnect(this.instanceId);

            if (ret < 0)
                throw WebSocketHelpers.GetErrorMessageFromCode(ret, null);
        }

        /// <summary>
        /// Close WebSocket connection with optional status code and reason.
        /// </summary>
        /// <param name="code">Close status code.</param>
        /// <param name="reason">Reason string.</param>
        public void Close(WebSocketCloseCode code = WebSocketCloseCode.Normal, string reason = null)
        {
            int ret = ElympicsWebSocketClose(this.instanceId, (int)code, reason);

            if (ret < 0)
                throw WebSocketHelpers.GetErrorMessageFromCode(ret, null);
        }

        /// <summary>
        /// Send binary data over the socket.
        /// </summary>
        /// <param name="data">Payload data.</param>
        public void Send(byte[] data)
        {
            int ret = ElympicsWebSocketSend(this.instanceId, data, data.Length);

            if (ret < 0)
                throw WebSocketHelpers.GetErrorMessageFromCode(ret, null);
        }

        /// <summary>
        /// Return WebSocket connection state.
        /// </summary>
        /// <returns>The state.</returns>
        public WebSocketState GetState()
        {
            int state = ElympicsWebSocketGetState(this.instanceId);

            if (state < 0)
                throw WebSocketHelpers.GetErrorMessageFromCode(state, null);

            switch (state)
            {
                case 0:
                    return WebSocketState.Connecting;

                case 1:
                    return WebSocketState.Open;

                case 2:
                    return WebSocketState.Closing;

                case 3:
                    return WebSocketState.Closed;

                default:
                    return WebSocketState.Closed;
            }
        }

        /// <summary>
        /// Delegates onOpen event from JSLIB to native sharp event
        /// Is called by WebSocketFactory
        /// </summary>
        public void DelegateOnOpenEvent()
        {
            OnOpen?.Invoke();
        }

        /// <summary>
        /// Delegates onMessage event from JSLIB to native sharp event
        /// Is called by WebSocketFactory
        /// </summary>
        /// <param name="data">Binary data.</param>
        public void DelegateOnMessageEvent(byte[] data)
        {
            OnMessage?.Invoke(data);
        }

        /// <summary>
        /// Delegates onError event from JSLIB to native sharp event
        /// Is called by WebSocketFactory
        /// </summary>
        /// <param name="errorMsg">Error message.</param>
        public void DelegateOnErrorEvent(string errorMsg)
        {
            OnError?.Invoke(errorMsg);
        }

        /// <summary>
        /// Delegate onClose event from JSLIB to native sharp event
        /// Is called by WebSocketFactory
        /// </summary>
        /// <param name="closeCode">Close status code.</param>
        public void DelegateOnCloseEvent(int closeCode, string reason)
        {
            OnClose?.Invoke(WebSocketHelpers.ParseCloseCodeEnum(closeCode), reason);
        }
    }
#else
    /// <summary>
    /// WebSocket class bound to <see cref="ClientWebSocket"/>.
    /// Events are raised on thread pool threads.
    /// </summary>
    public class WebSocket : IWebSocket
    {
        private const int ReceiveBufferSize = 16 * 1024;

        /// <summary>
        /// Occurs when the connection is opened.
        /// </summary>
        public event WebSocketOpenEventHandler OnOpen;

        /// <summary>
        /// Occurs when a message is received.
        /// </summary>
        public event WebSocketMessageEventHandler OnMessage;

        /// <summary>
        /// Occurs when an error was reported from WebSocket.
        /// </summary>
        public event WebSocketErrorEventHandler OnError;

        /// <summary>
        /// Occurs when the socked was closed.
        /// </summary>
        public event WebSocketCloseEventHandler OnClose;

        private readonly Uri _uri;
        private readonly string _protocol;
        private readonly object _lock = new();
        private readonly Queue<byte[]> _outgoing = new();
        private ClientWebSocket _ws;
        private CancellationTokenSource _cts;
        private WebSocketState _state = WebSocketState.Connecting;
        private bool _sending;
        private bool _closeReported;

        /// <summary>
        /// WebSocket constructor.
        /// </summary>
        /// <param name="url">Valid WebSocket URL.</param>
        /// <param name="protocol">Requested WebSocket sub-protocol.</param>
        internal WebSocket(string url, string protocol = null)
        {
            try
            {
                _uri = new Uri(url);
                _protocol = protocol;
            }
            catch (Exception e)
            {
                throw new WebSocketUnexpectedException("Failed to create WebSocket Client.", e);
            }
        }

        /// <summary>
        /// Open WebSocket connection
        /// </summary>
        public void Connect()
        {
            lock (_lock)
            {
                if (_state is WebSocketState.Open or WebSocketState.Closing)
                    throw new WebSocketInvalidStateException("WebSocket is already connected or is closing.");
                _state = WebSocketState.Connecting;
                _closeReported = false;
                _cts = new CancellationTokenSource();
                _ws = new ClientWebSocket();
                if (!string.IsNullOrEmpty(_protocol))
                    _ws.Options.AddSubProtocol(_protocol);
            }

            _ = RunAsync(_ws, _cts.Token);
        }

        /// <summary>
        /// Close WebSocket connection with optional status code and reason.
        /// </summary>
        /// <param name="code">Close status code.</param>
        /// <param name="reason">Reason string.</param>
        public void Close(WebSocketCloseCode code = WebSocketCloseCode.Normal, string reason = null)
        {
            ClientWebSocket ws;
            lock (_lock)
            {
                if (_state == WebSocketState.Closing)
                    throw new WebSocketInvalidStateException("WebSocket is already closing.");
                if (_state == WebSocketState.Closed)
                    throw new WebSocketInvalidStateException("WebSocket is already closed.");
                ws = _ws;
                _state = WebSocketState.Closing;
            }

            if (ws == null || ws.State != System.Net.WebSockets.WebSocketState.Open)
            {
                _cts?.Cancel();
                ReportClose(code, reason);
                return;
            }

            _ = CloseAsync(ws, code, reason);
        }

        /// <summary>
        /// Send binary data over the socket.
        /// </summary>
        /// <param name="data">Payload data.</param>
        public void Send(byte[] data)
        {
            lock (_lock)
            {
                if (_state != WebSocketState.Open)
                    throw new WebSocketInvalidStateException("WebSocket is not in open state.");
                _outgoing.Enqueue(data);
                if (_sending)
                    return;
                _sending = true;
            }

            _ = SendQueuedAsync(_ws, _cts.Token);
        }

        /// <summary>
        /// Return WebSocket connection state.
        /// </summary>
        /// <returns>The state.</returns>
        public WebSocketState GetState()
        {
            lock (_lock)
                return _state;
        }

        private async Task RunAsync(ClientWebSocket ws, CancellationToken ct)
        {
            try
            {
                await ws.ConnectAsync(_uri, ct).ConfigureAwait(false);
                lock (_lock)
                {
                    if (_state == WebSocketState.Connecting)
                        _state = WebSocketState.Open;
                }
                OnOpen?.Invoke();

                var buffer = new byte[ReceiveBufferSize];
                using var message = new MemoryStream();
                while (true)
                {
                    var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct).ConfigureAwait(false);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        if (ws.State == System.Net.WebSockets.WebSocketState.CloseReceived)
                            await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None).ConfigureAwait(false);
                        var code = result.CloseStatus.HasValue ? WebSocketHelpers.ParseCloseCodeEnum((int)result.CloseStatus.Value) : WebSocketCloseCode.NoStatus;
                        ReportClose(code, result.CloseStatusDescription ?? string.Empty);
                        return;
                    }

                    message.Write(buffer, 0, result.Count);
                    if (!result.EndOfMessage)
                        continue;
                    var data = message.ToArray();
                    message.SetLength(0);
                    OnMessage?.Invoke(data);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                ReportClose(WebSocketCloseCode.Abnormal, string.Empty);
            }
            catch (Exception e)
            {
                OnError?.Invoke(e.Message);
                ReportClose(WebSocketCloseCode.Abnormal, e.Message);
            }
        }

        private async Task SendQueuedAsync(ClientWebSocket ws, CancellationToken ct)
        {
            while (true)
            {
                byte[] data;
                lock (_lock)
                {
                    if (_outgoing.Count == 0 || _state != WebSocketState.Open)
                    {
                        _outgoing.Clear();
                        _sending = false;
                        return;
                    }
                    data = _outgoing.Dequeue();
                }

                try
                {
                    await ws.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, ct).ConfigureAwait(false);
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    OnError?.Invoke(e.Message);
                }
                catch (OperationCanceledException)
                {
                }
            }
        }

        private async Task CloseAsync(ClientWebSocket ws, WebSocketCloseCode code, string reason)
        {
            try
            {
                // The receive loop reports the close once the server acknowledges it.
                await ws.CloseOutputAsync((WebSocketCloseStatus)(int)code, reason, CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                OnError?.Invoke(e.Message);
                _cts?.Cancel();
                ReportClose(code, reason);
            }
        }

        private void ReportClose(WebSocketCloseCode code, string reason)
        {
            lock (_lock)
            {
                _state = WebSocketState.Closed;
                if (_closeReported)
                    return;
                _closeReported = true;
            }
            _ws?.Dispose();
            OnClose?.Invoke(code, reason ?? string.Empty);
        }
    }
#endif

    /// <summary>
    /// Class providing static access methods to work with JSLIB WebSocket or ClientWebSocket interface
    /// </summary>
    public static class WebSocketFactory
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        /* Map of websocket instances */
        private static readonly Dictionary<int, WebSocket> Instances = new Dictionary<int, WebSocket>();

        /* Delegates */
        public delegate void OnOpenCallback(int instanceId);
        public delegate void OnMessageCallback(int instanceId, IntPtr msgPtr, int msgSize);
        public delegate void OnErrorCallback(int instanceId, IntPtr errorPtr);
        public delegate void OnCloseCallback(int instanceId, int closeCode, IntPtr reasonPtr);

        /* WebSocket JSLIB callback setters and other functions */
        [DllImport("__Internal")]
        public static extern int ElympicsWebSocketAllocate(string url, string protocol);

        [DllImport("__Internal")]
        public static extern void ElympicsWebSocketFree(int instanceId);

        [DllImport("__Internal")]
        public static extern void ElympicsWebSocketSetOnOpen(OnOpenCallback callback);

        [DllImport("__Internal")]
        public static extern void ElympicsWebSocketSetOnMessage(OnMessageCallback callback);

        [DllImport("__Internal")]
        public static extern void ElympicsWebSocketSetOnError(OnErrorCallback callback);

        [DllImport("__Internal")]
        public static extern void ElympicsWebSocketSetOnClose(OnCloseCallback callback);

        /* If callbacks was initialized and set */
        private static bool isInitialized;

        /*
         * Initialize WebSocket callbacks to JSLIB
         */
        private static void Initialize()
        {
            ElympicsWebSocketSetOnOpen(DelegateOnOpenEvent);
            ElympicsWebSocketSetOnMessage(DelegateOnMessageEvent);
            ElympicsWebSocketSetOnError(DelegateOnErrorEvent);
            ElympicsWebSocketSetOnClose(DelegateOnCloseEvent);

            isInitialized = true;
        }

        /// <summary>
        /// Called when instance is destroyed (by destructor)
        /// Method removes instance from map and free it in JSLIB implementation
        /// </summary>
        /// <param name="instanceId">Instance identifier.</param>
        public static void HandleInstanceDestroy(int instanceId)
        {
            Instances.Remove(instanceId);
            ElympicsWebSocketFree(instanceId);
        }

        [MonoPInvokeCallback(typeof(OnOpenCallback))]
        public static void DelegateOnOpenEvent(int instanceId)
        {
            if (Instances.TryGetValue(instanceId, out var instanceRef))
            {
                instanceRef.DelegateOnOpenEvent();
            }
        }

        [MonoPInvokeCallback(typeof(OnMessageCallback))]
        public static void DelegateOnMessageEvent(int instanceId, IntPtr msgPtr, int msgSize)
        {
            if (Instances.TryGetValue(instanceId, out var instanceRef))
            {
                var msg = new byte[msgSize];
                Marshal.Copy(msgPtr, msg, 0, msgSize);

                instanceRef.DelegateOnMessageEvent(msg);
            }
        }

        [MonoPInvokeCallback(typeof(OnErrorCallback))]
        public static void DelegateOnErrorEvent(int instanceId, IntPtr errorPtr)
        {
            if (Instances.TryGetValue(instanceId, out var instanceRef))
            {
                var errorMsg = Marshal.PtrToStringUTF8(errorPtr);
                instanceRef.DelegateOnErrorEvent(errorMsg);
            }
        }

        [MonoPInvokeCallback(typeof(OnCloseCallback))]
        public static void DelegateOnCloseEvent(int instanceId, int closeCode, IntPtr reasonPtr)
        {
            if (Instances.TryGetValue(instanceId, out var instanceRef))
            {
                var reason = Marshal.PtrToStringUTF8(reasonPtr);
                instanceRef.DelegateOnCloseEvent(closeCode, reason);
            }
        }
#endif

        /// <summary>
        /// Create WebSocket client instance
        /// </summary>
        /// <returns>The WebSocket instance.</returns>
        /// <param name="url">WebSocket valid URL.</param>
        /// <param name="protocol">Requested WebSocket sub-protocol.</param>
        public static WebSocket CreateInstance(string url, string protocol = null)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
	        if (!isInitialized)
	            Initialize();

	        var instanceId = ElympicsWebSocketAllocate(url, protocol);
	        var wrapper = new WebSocket(instanceId);
	        Instances.Add(instanceId, wrapper);

	        return wrapper;
#else
            return new WebSocket(url, protocol);
#endif
        }
    }
}
