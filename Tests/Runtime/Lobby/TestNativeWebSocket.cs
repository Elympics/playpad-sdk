using System;
using System.Collections;
using System.IO;
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
using UnityEngine.TestTools;
using SystemWebSocket = System.Net.WebSockets.WebSocket;
using SystemWebSocketCloseStatus = System.Net.WebSockets.WebSocketCloseStatus;
using SystemWebSocketMessageType = System.Net.WebSockets.WebSocketMessageType;

namespace ElympicsPlayPad.Tests.Lobby
{
    /// <summary>The non-WebGL <see cref="WebSocket"/> against a local echo server.</summary>
    [Category("WebSocket")]
    public class TestNativeWebSocket
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

        // Unity's Mono doesn't implement HttpListenerContext.AcceptWebSocketAsync, so the echo server does the handshake itself.
        private TcpListener _listener;
        private string _url;
        private CancellationTokenSource _serverCts;

        [SetUp]
        public void SetUp()
        {
            _listener = new TcpListener(IPAddress.Loopback, 0);
            _listener.Start();
            _url = $"ws://127.0.0.1:{((IPEndPoint)_listener.LocalEndpoint).Port}/";
            _serverCts = new CancellationTokenSource();
            _ = Task.Run(() => ServeEchoAsync(_serverCts.Token));
        }

        [TearDown]
        public void TearDown()
        {
            _serverCts.Cancel();
            _listener.Stop();
        }

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
            Assert.That(ws.GetState(), Is.EqualTo(WebSocketState.Connecting));

            ws.Connect();
            await Step(opened.Task, "open", () => error);
            Assert.That(ws.GetState(), Is.EqualTo(WebSocketState.Open));

            var smallPayload = new byte[] { 1, 2, 3 };
            ws.Send(smallPayload);
            Assert.That(await Step(received.Task, "small echo"), Is.EqualTo(smallPayload));

            received = new UniTaskCompletionSource<byte[]>();
            var payload = new byte[40000];
            new Random(7).NextBytes(payload);
            ws.Send(payload);
            Assert.That(await Step(received.Task, "echo"), Is.EqualTo(payload));

            ws.Close(WebSocketCloseCode.Normal, "bye");
            Assert.That(await Step(closed.Task, "close"), Is.EqualTo(WebSocketCloseCode.Normal));
            Assert.That(ws.GetState(), Is.EqualTo(WebSocketState.Closed));
            _ = Assert.Throws<WebSocketInvalidStateException>(() => ws.Send(payload));
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

        private static int GetFreePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private async Task ServeEchoAsync(CancellationToken ct)
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
                _ = Task.Run(() => EchoAsync(socket, ct), ct);
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
    }
}
