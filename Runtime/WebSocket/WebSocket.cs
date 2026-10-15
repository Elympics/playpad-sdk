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
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
using AOT;
#else
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
        private static readonly TimeSpan CloseTimeout = TimeSpan.FromSeconds(5);

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
        private Run _run;
        private WebSocketState _state = WebSocketState.Connecting;

        // One per Connect, so a previous run that finishes late can't touch the state or socket of the current one.
        private sealed class Run
        {
            public readonly ClientWebSocket Ws = new();
            public readonly CancellationTokenSource Cts = new();
            public readonly Queue<byte[]> Outgoing = new();
            public bool Sending;
            public bool CloseReported;
        }

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
            Run previous;
            Run run;
            lock (_lock)
            {
                if (_state is WebSocketState.Open or WebSocketState.Closing)
                    throw new WebSocketInvalidStateException("WebSocket is already connected or is closing.");
                _state = WebSocketState.Connecting;
                previous = _run;
                run = _run = new Run();
                if (!string.IsNullOrEmpty(_protocol))
                    run.Ws.Options.AddSubProtocol(_protocol);
            }

            previous?.Cts.Cancel();
            _ = RunAsync(run);
        }

        /// <summary>
        /// Close WebSocket connection with optional status code and reason.
        /// </summary>
        /// <param name="code">Close status code.</param>
        /// <param name="reason">Reason string.</param>
        public void Close(WebSocketCloseCode code = WebSocketCloseCode.Normal, string reason = null)
        {
            Run run;
            lock (_lock)
            {
                if (_state == WebSocketState.Closing)
                    throw new WebSocketInvalidStateException("WebSocket is already closing.");
                if (_state == WebSocketState.Closed)
                    throw new WebSocketInvalidStateException("WebSocket is already closed.");
                run = _run;
                _state = run == null ? WebSocketState.Closed : WebSocketState.Closing;
            }

            if (run == null)
            {
                OnClose?.Invoke(code, reason ?? string.Empty);
                return;
            }

            if (run.Ws.State != System.Net.WebSockets.WebSocketState.Open)
            {
                // Report before cancelling: the cancellation may run the receive loop inline, which would report Abnormal.
                ReportClose(run, code, reason);
                run.Cts.Cancel();
                return;
            }

            _ = CloseAsync(run, code, reason);
        }

        /// <summary>
        /// Send binary data over the socket.
        /// </summary>
        /// <param name="data">Payload data.</param>
        public void Send(byte[] data)
        {
            Run run;
            lock (_lock)
            {
                if (_state != WebSocketState.Open)
                    throw new WebSocketInvalidStateException("WebSocket is not in open state.");
                run = _run;
                run.Outgoing.Enqueue(data);
                if (run.Sending)
                    return;
                run.Sending = true;
            }

            _ = SendQueuedAsync(run);
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

        private async Task RunAsync(Run run)
        {
            var ws = run.Ws;
            var ct = run.Cts.Token;
            try
            {
                await ws.ConnectAsync(_uri, ct).ConfigureAwait(false);
                bool opened;
                lock (_lock)
                {
                    opened = _run == run && _state == WebSocketState.Connecting;
                    if (opened)
                        _state = WebSocketState.Open;
                }
                if (opened)
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
                        ReportClose(run, code, result.CloseStatusDescription ?? string.Empty);
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
                ReportClose(run, WebSocketCloseCode.Abnormal, string.Empty);
            }
            catch (Exception e)
            {
                ReportError(run, e.Message);
                ReportClose(run, WebSocketCloseCode.Abnormal, e.Message);
            }
            finally
            {
                ws.Dispose();
            }
        }

        private async Task SendQueuedAsync(Run run)
        {
            while (true)
            {
                byte[] data;
                lock (_lock)
                {
                    if (run.Outgoing.Count == 0 || _run != run || _state != WebSocketState.Open)
                    {
                        run.Outgoing.Clear();
                        run.Sending = false;
                        return;
                    }
                    data = run.Outgoing.Dequeue();
                }

                try
                {
                    await run.Ws.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, run.Cts.Token).ConfigureAwait(false);
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    ReportError(run, e.Message);
                }
                catch (OperationCanceledException)
                {
                }
            }
        }

        private async Task CloseAsync(Run run, WebSocketCloseCode code, string reason)
        {
            try
            {
                // The receive loop reports the close once the server acknowledges it, or Abnormal once the timeout cancels it.
                await run.Ws.CloseOutputAsync((WebSocketCloseStatus)(int)code, reason, CancellationToken.None).ConfigureAwait(false);
                run.Cts.CancelAfter(CloseTimeout);
            }
            catch (Exception e)
            {
                ReportError(run, e.Message);
                ReportClose(run, code, reason);
                run.Cts.Cancel();
            }
        }

        private void ReportError(Run run, string message)
        {
            lock (_lock)
            {
                if (_run != run || run.CloseReported)
                    return;
            }
            OnError?.Invoke(message);
        }

        private void ReportClose(Run run, WebSocketCloseCode code, string reason)
        {
            lock (_lock)
            {
                if (_run != run || run.CloseReported)
                    return;
                run.CloseReported = true;
                _state = WebSocketState.Closed;
            }
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
