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
using System.Text;
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
    /// <remarks>
    /// An instance is single-use: its JSLIB instance is freed once it closes, or when <see cref="Close"/> finds it unconnected or already closed.
    /// To reconnect, create a new instance with <see cref="WebSocketFactory.CreateInstance"/>, also from an <see cref="OnClose"/> handler.
    /// </remarks>
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

        private bool isFreed;

        /// <inheritdoc/>
        public event WebSocketOpenEventHandler OnOpen;

        /// <inheritdoc/>
        public event WebSocketMessageEventHandler OnMessage;

        /// <inheritdoc/>
        public event WebSocketErrorEventHandler OnError;

        /// <inheritdoc/>
        public event WebSocketCloseEventHandler OnClose;

        /// <summary>
        /// Constructor - receive JSLIB instance id of allocated socket
        /// </summary>
        /// <param name="instanceId">Instance identifier.</param>
        internal WebSocket(int instanceId)
        {
            this.instanceId = instanceId;
        }

        internal void Free()
        {
            if (isFreed)
                return;
            isFreed = true;
            WebSocketFactory.HandleInstanceDestroy(this.instanceId);
        }

        /// <inheritdoc/>
        public void Connect()
        {
            if (isFreed)
                throw WebSocketHelpers.GetErrorMessageFromCode(-5, null);

            int ret = ElympicsWebSocketConnect(this.instanceId);

            if (ret < 0)
                throw WebSocketHelpers.GetErrorMessageFromCode(ret, null);
        }

        /// <inheritdoc/>
        public void Close(WebSocketCloseCode code = WebSocketCloseCode.Normal, string reason = null)
        {
            if (isFreed)
                throw WebSocketHelpers.GetErrorMessageFromCode(-5, null);

            int ret = ElympicsWebSocketClose(this.instanceId, (int)code, reason);

            if (ret is -3 or -5)
                Free();

            if (ret < 0)
                throw WebSocketHelpers.GetErrorMessageFromCode(ret, null);
        }

        /// <inheritdoc/>
        public void Send(byte[] data)
        {
            if (isFreed)
                throw WebSocketHelpers.GetErrorMessageFromCode(-6, null);

            int ret = ElympicsWebSocketSend(this.instanceId, data, data.Length);

            if (ret < 0)
                throw WebSocketHelpers.GetErrorMessageFromCode(ret, null);
        }

        /// <inheritdoc/>
        public WebSocketState GetState()
        {
            if (isFreed)
                return WebSocketState.Closed;

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
    /// Events are raised on thread pool threads. Handler exceptions are logged and don't affect the connection.
    /// </summary>
    /// <remarks>
    /// An instance is single-use, like the WebGL one: to reconnect, create a new instance with <see cref="WebSocketFactory.CreateInstance"/>.
    /// </remarks>
    public class WebSocket : IWebSocket
    {
        private const int ReceiveBufferSize = 16 * 1024;
        private const int MaxCloseReasonBytes = 123;

        /// <inheritdoc/>
        public event WebSocketOpenEventHandler OnOpen;

        /// <inheritdoc/>
        public event WebSocketMessageEventHandler OnMessage;

        /// <inheritdoc/>
        public event WebSocketErrorEventHandler OnError;

        /// <inheritdoc/>
        public event WebSocketCloseEventHandler OnClose;

        internal TimeSpan CloseTimeout { get; set; } = TimeSpan.FromSeconds(5);

        private readonly Uri _uri;
        private readonly string _protocol;
        private readonly object _lock = new();
        private Run _run;
        private WebSocketState _state = WebSocketState.Closed;

        private sealed class Run
        {
            public readonly ClientWebSocket Ws = new();
            public readonly CancellationTokenSource Cts = new();
            // The source is disposed when the run ends, which makes reading its Token throw.
            public readonly CancellationToken Token;
            public readonly Queue<byte[]> Outgoing = new();
            // The send loop is the only writer, so a close write never overlaps a send. It stays set once the loop takes the close.
            public bool Sending;
            public CloseRequest Close;
            public bool CloseReported;

            public Run() => Token = Cts.Token;
        }

        private sealed class CloseRequest
        {
            public readonly WebSocketCloseCode Code;
            public readonly string Reason;
            public readonly bool Remote;
            public readonly TaskCompletionSource<bool> Done = new(TaskCreationOptions.RunContinuationsAsynchronously);

            public CloseRequest(WebSocketCloseCode code, string reason, bool remote)
            {
                Code = code;
                Reason = reason;
                Remote = remote;
            }
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

        /// <inheritdoc/>
        public void Connect()
        {
            Run run;
            lock (_lock)
            {
                if (_run != null)
                    throw WebSocketHelpers.GetErrorMessageFromCode(-2, null);
                _state = WebSocketState.Connecting;
                run = _run = new Run();
                if (!string.IsNullOrEmpty(_protocol))
                    run.Ws.Options.AddSubProtocol(_protocol);
            }

            _ = RunAsync(run);
        }

        /// <inheritdoc/>
        public void Close(WebSocketCloseCode code = WebSocketCloseCode.Normal, string reason = null)
        {
            Run run;
            lock (_lock)
            {
                if (_run == null)
                    throw WebSocketHelpers.GetErrorMessageFromCode(-3, null);
                if (_state == WebSocketState.Closing)
                    throw WebSocketHelpers.GetErrorMessageFromCode(-4, null);
                if (_state == WebSocketState.Closed)
                    throw WebSocketHelpers.GetErrorMessageFromCode(-5, null);
                if (!IsValidCloseCode(code) || (reason != null && Encoding.UTF8.GetByteCount(reason) > MaxCloseReasonBytes))
                    throw WebSocketHelpers.GetErrorMessageFromCode(-7, null);
                run = _run;
                _state = WebSocketState.Closing;
            }

            if (run.Ws.State != System.Net.WebSockets.WebSocketState.Open)
            {
                // Report before cancelling: the cancellation may run the receive loop inline, which would report Abnormal.
                ReportClose(run, code, reason);
                Cancel(run);
                return;
            }

            _ = RequestClose(run, new CloseRequest(code, reason, false));
        }

        /// <inheritdoc/>
        public void Send(byte[] data)
        {
            Run run;
            lock (_lock)
            {
                if (_state != WebSocketState.Open)
                    throw WebSocketHelpers.GetErrorMessageFromCode(-6, null);
                run = _run;
                run.Outgoing.Enqueue((byte[])data.Clone());
                if (run.Sending)
                    return;
                run.Sending = true;
            }

            _ = WriteAsync(run);
        }

        /// <inheritdoc/>
        public WebSocketState GetState()
        {
            lock (_lock)
                return _state;
        }

        // Browsers accept only 1000 and 3000-4999.
        private static bool IsValidCloseCode(WebSocketCloseCode code) => code == WebSocketCloseCode.Normal || (int)code is >= 3000 and <= 4999;

        private async Task RunAsync(Run run)
        {
            var ws = run.Ws;
            var ct = run.Token;
            try
            {
                await ws.ConnectAsync(_uri, ct).ConfigureAwait(false);
                bool opened;
                lock (_lock)
                {
                    opened = _state == WebSocketState.Connecting;
                    if (opened)
                        _state = WebSocketState.Open;
                }
                if (opened)
                    RaiseOpen();

                var buffer = new byte[ReceiveBufferSize];
                using var message = new MemoryStream();
                while (true)
                {
                    var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct).ConfigureAwait(false);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        if (ws.State == System.Net.WebSockets.WebSocketState.CloseReceived)
                            await AcknowledgeCloseAsync(run).ConfigureAwait(false);
                        var code = result.CloseStatus.HasValue ? WebSocketHelpers.ParseCloseCodeEnum((int)result.CloseStatus.Value) : WebSocketCloseCode.NoStatus;
                        ReportClose(run, code, result.CloseStatusDescription ?? string.Empty);
                        return;
                    }

                    if (result.EndOfMessage && message.Length == 0)
                    {
                        RaiseMessage(buffer.AsSpan(0, result.Count).ToArray());
                        continue;
                    }

                    message.Write(buffer, 0, result.Count);
                    if (!result.EndOfMessage)
                        continue;
                    var data = message.ToArray();
                    message.SetLength(0);
                    RaiseMessage(data);
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
                run.Cts.Dispose();
            }
        }

        private async Task AcknowledgeCloseAsync(Run run)
        {
            lock (_lock)
            {
                if (_state == WebSocketState.Open)
                    _state = WebSocketState.Closing;
                run.Outgoing.Clear();
            }

            var request = RequestClose(run, new CloseRequest(WebSocketCloseCode.Normal, null, true));
            // A close reported without the write loop (Close on a socket that isn't open) cancels the run instead.
            using var registration = run.Token.Register(() => request.Done.TrySetCanceled(run.Token));
            _ = await request.Done.Task.ConfigureAwait(false);
        }

        private CloseRequest RequestClose(Run run, CloseRequest request)
        {
            var start = false;
            lock (_lock)
            {
                if (run.Close != null)
                    return run.Close;
                run.Close = request;
                if (!run.Sending)
                    run.Sending = start = true;
            }

            if (start)
                _ = WriteAsync(run);
            return request;
        }

        private async Task WriteAsync(Run run)
        {
            while (true)
            {
                byte[] data = null;
                CloseRequest close = null;
                lock (_lock)
                {
                    if (run.CloseReported)
                    {
                        run.Outgoing.Clear();
                        run.Sending = false;
                        return;
                    }

                    if (run.Outgoing.Count > 0)
                        data = run.Outgoing.Dequeue();
                    else if (run.Close != null)
                        close = run.Close;
                    else
                    {
                        run.Sending = false;
                        return;
                    }
                }

                if (close != null)
                {
                    await WriteCloseAsync(run, close).ConfigureAwait(false);
                    return;
                }

                try
                {
                    await run.Ws.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, run.Token).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    lock (_lock)
                        run.Outgoing.Clear();
                    if (e is not OperationCanceledException)
                        ReportError(run, e.Message);
                }
            }
        }

        private async Task WriteCloseAsync(Run run, CloseRequest close)
        {
            try
            {
                // The receive loop reports the close once the server acknowledges it. The timeout covers this write and that wait.
                CancelAfter(run, CloseTimeout);
                await run.Ws.CloseOutputAsync((WebSocketCloseStatus)(int)close.Code, close.Reason, run.Token).ConfigureAwait(false);
                _ = close.Done.TrySetResult(true);
            }
            catch (Exception e)
            {
                if (close.Remote)
                {
                    _ = close.Done.TrySetException(e);
                    return;
                }

                _ = close.Done.TrySetResult(false);
                if (e is OperationCanceledException)
                {
                    ReportClose(run, WebSocketCloseCode.Abnormal, string.Empty);
                    return;
                }

                ReportError(run, e.Message);
                ReportClose(run, close.Code, close.Reason);
                Cancel(run);
            }
        }

        private static void Cancel(Run run)
        {
            try
            { run.Cts.Cancel(); }
            catch (ObjectDisposedException) { }
        }

        private static void CancelAfter(Run run, TimeSpan delay)
        {
            try
            { run.Cts.CancelAfter(delay); }
            catch (ObjectDisposedException) { }
        }

        private void ReportError(Run run, string message)
        {
            lock (_lock)
            {
                if (run.CloseReported)
                    return;
            }
            RaiseError(message);
        }

        private void ReportClose(Run run, WebSocketCloseCode code, string reason)
        {
            lock (_lock)
            {
                if (run.CloseReported)
                    return;
                run.CloseReported = true;
                _state = WebSocketState.Closed;
            }
            RaiseClose(code, reason ?? string.Empty);
        }

        private void RaiseOpen()
        {
            try
            { OnOpen?.Invoke(); }
            catch (Exception e) { UnityEngine.Debug.LogException(e); }
        }

        private void RaiseMessage(byte[] data)
        {
            try
            { OnMessage?.Invoke(data); }
            catch (Exception e) { UnityEngine.Debug.LogException(e); }
        }

        private void RaiseError(string message)
        {
            try
            { OnError?.Invoke(message); }
            catch (Exception e) { UnityEngine.Debug.LogException(e); }
        }

        private void RaiseClose(WebSocketCloseCode code, string reason)
        {
            try
            { OnClose?.Invoke(code, reason); }
            catch (Exception e) { UnityEngine.Debug.LogException(e); }
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
        /// Called when the instance is freed.
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
                try
                {
                    instanceRef.DelegateOnCloseEvent(closeCode, reason);
                }
                finally
                {
                    instanceRef.Free();
                }
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
