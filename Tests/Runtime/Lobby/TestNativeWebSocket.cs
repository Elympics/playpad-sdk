using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Elympics.Cysharp.Threading.Tasks;
using ElympicsPlayPad.WebSockets;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using SystemWebSocket = System.Net.WebSockets.WebSocket;
using SystemWebSocketCloseStatus = System.Net.WebSockets.WebSocketCloseStatus;
using SystemWebSocketMessageType = System.Net.WebSockets.WebSocketMessageType;

namespace ElympicsPlayPad.Tests.Lobby
{
    /// <summary>The non-WebGL <see cref="WebSocket"/> against a local server.</summary>
    [Category("WebSocket")]
    public class TestNativeWebSocket
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

        // Unity's Mono doesn't implement HttpListenerContext.AcceptWebSocketAsync, so the server does the handshake itself.
        private TcpListener _listener;
        private string _url;
        private CancellationTokenSource _serverCts;
        private Func<SystemWebSocket, CancellationToken, Task> _serverBehavior;

        [SetUp]
        public void SetUp()
        {
            _listener = StartLoopbackListener();
            _url = $"ws://127.0.0.1:{GetPort(_listener)}/";
            _serverCts = new CancellationTokenSource();
            _serverBehavior = EchoAsync;
            _ = Task.Run(() => ServeAsync(_serverCts.Token));
        }

        [TearDown]
        public void TearDown()
        {
            _serverCts.Cancel();
            _listener.Stop();
        }

        [Test]
        public void NewSocketIsClosedAndCannotBeClosed()
        {
            var ws = WebSocketFactory.CreateInstance(_url);

            Assert.That(ws.GetState(), Is.EqualTo(WebSocketState.Closed));
            _ = Assert.Throws<WebSocketInvalidStateException>(() => ws.Close());
            _ = Assert.Throws<WebSocketInvalidStateException>(() => ws.Send(new byte[] { 1 }));
        }

        [UnityTest]
        public IEnumerator SecondConnectThrows() => UniTask.ToCoroutine(async () =>
        {
            var ws = WebSocketFactory.CreateInstance(_url);
            var opened = new UniTaskCompletionSource();
            ws.OnOpen += () => opened.TrySetResult();

            ws.Connect();
            Assert.That(ws.GetState(), Is.EqualTo(WebSocketState.Connecting));
            _ = Assert.Throws<WebSocketInvalidStateException>(() => ws.Connect());

            await Step(opened.Task, "open");
            _ = Assert.Throws<WebSocketInvalidStateException>(() => ws.Connect());
            ws.Close();
        });

        [UnityTest]
        public IEnumerator ReconnectingAfterCloseThrows() => UniTask.ToCoroutine(async () =>
        {
            var ws = WebSocketFactory.CreateInstance(_url);
            var opened = new UniTaskCompletionSource();
            var closed = new UniTaskCompletionSource<WebSocketCloseCode>();
            ws.OnOpen += () => opened.TrySetResult();
            ws.OnClose += (code, _) => closed.TrySetResult(code);
            ws.Connect();
            await Step(opened.Task, "open");
            ws.Close();
            _ = await Step(closed.Task, "close");

            _ = Assert.Throws<WebSocketInvalidStateException>(() => ws.Connect());
            _ = Assert.Throws<WebSocketInvalidStateException>(() => ws.Close());
        });

        [UnityTest]
        public IEnumerator InvalidCloseArgumentsThrowAndKeepSocketOpen() => UniTask.ToCoroutine(async () =>
        {
            var ws = WebSocketFactory.CreateInstance(_url);
            var opened = new UniTaskCompletionSource();
            ws.OnOpen += () => opened.TrySetResult();
            ws.Connect();
            await Step(opened.Task, "open");

            _ = Assert.Throws<WebSocketInvalidArgumentException>(() => ws.Close(WebSocketCloseCode.Abnormal));
            _ = Assert.Throws<WebSocketInvalidArgumentException>(() => ws.Close(WebSocketCloseCode.Normal, new string('x', 124)));

            Assert.That(ws.GetState(), Is.EqualTo(WebSocketState.Open));
            ws.Close();
        });

        [UnityTest]
        public IEnumerator EchoesBinaryMessagesAndClosesNormally() => UniTask.ToCoroutine(async () =>
        {
            var ws = WebSocketFactory.CreateInstance(_url, "test-protocol");
            var opened = new UniTaskCompletionSource();
            var received = new UniTaskCompletionSource<byte[]>();
            var closed = new UniTaskCompletionSource<WebSocketCloseCode>();
            ws.OnOpen += () => opened.TrySetResult();
            string error = null;
            ws.OnError += e => error = e;
            ws.OnMessage += data => received.TrySetResult(data);
            ws.OnClose += (code, _) => closed.TrySetResult(code);
            Assert.That(ws.GetState(), Is.EqualTo(WebSocketState.Closed));

            ws.Connect();
            Assert.That(ws.GetState(), Is.EqualTo(WebSocketState.Connecting));
            await Step(opened.Task, "open", () => error);
            Assert.That(ws.GetState(), Is.EqualTo(WebSocketState.Open));

            var smallPayload = new byte[] { 1, 2, 3 };
            ws.Send(smallPayload);
            Assert.That(await Step(received.Task, "small echo"), Is.EqualTo(smallPayload));

            received = new UniTaskCompletionSource<byte[]>();
            var payload = new byte[40000];
            new System.Random(7).NextBytes(payload);
            ws.Send(payload);
            Assert.That(await Step(received.Task, "echo"), Is.EqualTo(payload));

            ws.Close(WebSocketCloseCode.Normal, "bye");
            Assert.That(await Step(closed.Task, "close"), Is.EqualTo(WebSocketCloseCode.Normal));
            Assert.That(ws.GetState(), Is.EqualTo(WebSocketState.Closed));
            _ = Assert.Throws<WebSocketInvalidStateException>(() => ws.Send(payload));
        });

        [UnityTest]
        public IEnumerator SendFollowedByCloseDeliversAllMessagesFirst() => UniTask.ToCoroutine(async () =>
        {
            var ws = WebSocketFactory.CreateInstance(_url);
            var messages = new ConcurrentQueue<byte[]>();
            var opened = new UniTaskCompletionSource();
            var closed = new UniTaskCompletionSource<WebSocketCloseCode>();
            ws.OnOpen += () => opened.TrySetResult();
            ws.OnMessage += messages.Enqueue;
            ws.OnClose += (code, _) => closed.TrySetResult(code);
            ws.Connect();
            await Step(opened.Task, "open");

            var payloads = Enumerable.Range(0, 20).Select(i => new[] { (byte)i, (byte)(i * 2) }).ToArray();
            foreach (var payload in payloads)
                ws.Send(payload);
            ws.Close();

            Assert.That(await Step(closed.Task, "close"), Is.EqualTo(WebSocketCloseCode.Normal));
            Assert.That(messages, Is.EqualTo(payloads));
        });

        [UnityTest]
        public IEnumerator CloseAgainstServerThatNeverAcknowledgesTimesOutAsAbnormal() => UniTask.ToCoroutine(async () =>
        {
            _serverBehavior = SilentAsync;
            var ws = WebSocketFactory.CreateInstance(_url);
            ws.CloseTimeout = TimeSpan.FromMilliseconds(300);
            var opened = new UniTaskCompletionSource();
            var closed = new UniTaskCompletionSource<WebSocketCloseCode>();
            ws.OnOpen += () => opened.TrySetResult();
            ws.OnClose += (code, _) => closed.TrySetResult(code);
            ws.Connect();
            await Step(opened.Task, "open");

            ws.Close();

            Assert.That(await Step(closed.Task, "close"), Is.EqualTo(WebSocketCloseCode.Abnormal));
            Assert.That(ws.GetState(), Is.EqualTo(WebSocketState.Closed));
        });

        [UnityTest]
        public IEnumerator ServerInitiatedCloseReportsItsCodeAndReason() => UniTask.ToCoroutine(async () =>
        {
            _serverBehavior = (socket, ct) => socket.CloseAsync(SystemWebSocketCloseStatus.PolicyViolation, "not allowed", ct);
            var ws = WebSocketFactory.CreateInstance(_url);
            var closed = new UniTaskCompletionSource<(WebSocketCloseCode, string)>();
            ws.OnClose += (code, reason) => closed.TrySetResult((code, reason));

            ws.Connect();

            var (closeCode, closeReason) = await Step(closed.Task, "close");
            Assert.That(closeCode, Is.EqualTo(WebSocketCloseCode.PolicyViolation));
            Assert.That(closeReason, Is.EqualTo("not allowed"));
            Assert.That(ws.GetState(), Is.EqualTo(WebSocketState.Closed));
        });

        [UnityTest]
        public IEnumerator FragmentedEmptyAndTextFramesAreDeliveredAsBytes() => UniTask.ToCoroutine(async () =>
        {
            _serverBehavior = async (socket, ct) =>
            {
                await socket.SendAsync(new ArraySegment<byte>(new byte[] { 1, 2 }), SystemWebSocketMessageType.Binary, false, ct);
                await socket.SendAsync(new ArraySegment<byte>(new byte[] { 3 }), SystemWebSocketMessageType.Binary, false, ct);
                await socket.SendAsync(new ArraySegment<byte>(new byte[] { 4, 5 }), SystemWebSocketMessageType.Binary, true, ct);
                await socket.SendAsync(new ArraySegment<byte>(Array.Empty<byte>()), SystemWebSocketMessageType.Binary, true, ct);
                await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("zażółć")), SystemWebSocketMessageType.Text, true, ct);
                await SilentAsync(socket, ct);
            };
            var ws = WebSocketFactory.CreateInstance(_url);
            var messages = new ConcurrentQueue<byte[]>();
            ws.OnMessage += messages.Enqueue;

            ws.Connect();
            await Step(UniTask.WaitUntil(() => messages.Count >= 3), "messages");

            Assert.That(messages.ToArray(), Is.EqualTo(new[] { new byte[] { 1, 2, 3, 4, 5 }, Array.Empty<byte>(), Encoding.UTF8.GetBytes("zażółć") }));
            ws.Close();
        });

        [UnityTest]
        public IEnumerator ThrowingMessageHandlerDoesNotCloseTheSocket() => UniTask.ToCoroutine(async () =>
        {
            LogAssert.Expect(LogType.Exception, new Regex("handler failure"));
            var ws = WebSocketFactory.CreateInstance(_url);
            var opened = new UniTaskCompletionSource();
            var received = new UniTaskCompletionSource<byte[]>();
            var closed = false;
            var calls = 0;
            ws.OnOpen += () => opened.TrySetResult();
            ws.OnClose += (_, _) => closed = true;
            ws.OnMessage += data =>
            {
                if (Interlocked.Increment(ref calls) == 1)
                    throw new InvalidOperationException("handler failure");
                _ = received.TrySetResult(data);
            };
            ws.Connect();
            await Step(opened.Task, "open");

            ws.Send(new byte[] { 1 });
            ws.Send(new byte[] { 2 });

            Assert.That(await Step(received.Task, "second message"), Is.EqualTo(new byte[] { 2 }));
            Assert.That(closed, Is.False);
            Assert.That(ws.GetState(), Is.EqualTo(WebSocketState.Open));
            ws.Close();
        });

        [UnityTest]
        public IEnumerator FailedConnectionReportsErrorThenAbnormalClose() => UniTask.ToCoroutine(async () =>
        {
            var ws = WebSocketFactory.CreateInstance($"ws://localhost:{GetFreePort()}/");
            var errored = false;
            var closed = new UniTaskCompletionSource<WebSocketCloseCode>();
            ws.OnError += _ => errored = true;
            ws.OnClose += (code, _) => closed.TrySetResult(code);

            ws.Connect();

            Assert.That(await closed.Task.Timeout(Timeout), Is.EqualTo(WebSocketCloseCode.Abnormal));
            Assert.That(errored, Is.True);
            Assert.That(ws.GetState(), Is.EqualTo(WebSocketState.Closed));
        });

        private static UniTask Step(UniTask task, string name, Func<string> error = null) => Step(task.AsAsyncUnitUniTask(), name, error);

        private static async UniTask<T> Step<T>(UniTask<T> task, string name, Func<string> error = null)
        {
            try
            { return await task.Timeout(Timeout); }
            catch (TimeoutException) { Assert.Fail($"Timed out waiting for {name}. Error: {error?.Invoke() ?? "none"}"); return default; }
        }

        private static TcpListener StartLoopbackListener()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            return listener;
        }

        private static int GetPort(TcpListener listener) => ((IPEndPoint)listener.LocalEndpoint).Port;

        private static int GetFreePort()
        {
            var listener = StartLoopbackListener();
            var port = GetPort(listener);
            listener.Stop();
            return port;
        }

        private async Task ServeAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                TcpClient client;
                try
                {
                    client = await _listener.AcceptTcpClientAsync();
                }
                catch (Exception) when (ct.IsCancellationRequested)
                {
                    return;
                }

                var stream = client.GetStream();
                var subProtocol = await HandshakeAsync(stream);
                var socket = SystemWebSocket.CreateFromStream(stream, true, subProtocol, TimeSpan.FromSeconds(30));
                var behavior = _serverBehavior;
                _ = Task.Run(async () =>
                {
                    try
                    { await behavior(socket, ct); }
                    catch (Exception) { /* The client side asserts; the server just stops. */ }
                }, ct);
            }
        }

        /// <returns>The requested sub-protocol, accepted as is.</returns>
        private static async Task<string> HandshakeAsync(Stream stream)
        {
            var request = new StringBuilder();
            var buffer = new byte[1];
            while (!request.ToString().EndsWith("\r\n\r\n"))
            {
                if (await stream.ReadAsync(buffer, 0, 1) == 0)
                    throw new EndOfStreamException();
                _ = request.Append((char)buffer[0]);
            }

            string Header(string name) => Regex.Match(request.ToString(), $@"^{name}:\s*(.*?)\s*$",
                RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups[1].Value;

            using var sha1 = SHA1.Create();
            var accept = Convert.ToBase64String(sha1.ComputeHash(Encoding.ASCII.GetBytes(Header("Sec-WebSocket-Key") + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
            var subProtocol = Header("Sec-WebSocket-Protocol");
            var response = "HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\nConnection: Upgrade\r\n"
                + $"Sec-WebSocket-Accept: {accept}\r\n" + (subProtocol.Length > 0 ? $"Sec-WebSocket-Protocol: {subProtocol}\r\n" : "") + "\r\n";
            var bytes = Encoding.ASCII.GetBytes(response);
            await stream.WriteAsync(bytes, 0, bytes.Length);
            return subProtocol.Length > 0 ? subProtocol : null;
        }

        private static async Task EchoAsync(SystemWebSocket socket, CancellationToken ct)
        {
            var buffer = new byte[64 * 1024];
            var message = new MemoryStream();
            while (!ct.IsCancellationRequested)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                if (result.MessageType == SystemWebSocketMessageType.Close)
                {
                    await socket.CloseAsync(result.CloseStatus ?? SystemWebSocketCloseStatus.NormalClosure, result.CloseStatusDescription, ct);
                    return;
                }
                message.Write(buffer, 0, result.Count);
                if (!result.EndOfMessage)
                    continue;
                await socket.SendAsync(new ArraySegment<byte>(message.ToArray()), result.MessageType, true, ct);
                message.SetLength(0);
            }
        }

        /// <summary>Reads everything and never answers a close frame.</summary>
        private static async Task SilentAsync(SystemWebSocket socket, CancellationToken ct)
        {
            var buffer = new byte[1024];
            while (!ct.IsCancellationRequested)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                if (result.MessageType == SystemWebSocketMessageType.Close)
                    await Task.Delay(System.Threading.Timeout.Infinite, ct);
            }
        }
    }
}
