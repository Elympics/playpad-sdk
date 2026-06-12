using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js
{
    internal static class PlayPadExtensionConnection
    {
        public static event Action Disconnected;

        public static event Action<byte[]> MessageReceived;
        public static bool IsConnected => client is { Connected: true };
        private const int UnityPort = 54535;
        private static TcpClient client;
        private static NetworkStream stream;
        private static readonly byte[] MessageLenghtBuffer = new byte[4];
        private static CancellationTokenSource cts;

        public static void ConnectToExtension()
        {
            if (IsConnected)
            {
                Debug.Log("Unity - PlayPad host connection is already established.");
                return;
            }
            cts = new CancellationTokenSource();
            InitialConnectionFlow(cts.Token);
        }

        private static void InitialConnectionFlow(CancellationToken token)
        {
            OpenConnection();
            if (token.IsCancellationRequested || client == null)
                return;

            SetupStream();
            if (token.IsCancellationRequested || client == null)
                return;
            ReadUnityMessages(token).Forget(Debug.LogException);
        }
        private static void SetupStream()
        {
            if (!client.Connected)
            {
                throw new ElympicsException("Can't connect to PlayPad extension.");
            }
            stream = client.GetStream();
        }

        public static void DisconnectFromExtension() => cts?.Cancel();

        private static void OpenConnection() => client = EstablishConnection();

        private static TcpClient EstablishConnection()
        {
            client = new TcpClient();
            try
            {
                Debug.Log("Connecting to Extension...");
                client!.Connect(IPAddress.Loopback, UnityPort);
                Debug.Log("Extension connection established.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Couldn't establish connection to PlayPad extension. Please make sure that extension manifest is properly installed via \"Tools/PlayPad/PlayPad Extension Installation Window\". Error: {e.Message}");
                throw;
            }
            return client;
        }

        private static async UniTask ReadUnityMessages(CancellationToken ct)
        {
            try
            {
                Debug.Log("Ready to read messages from PlayPad extension...");
                while (client.Connected)
                {
                    try
                    {
                        if (ct.IsCancellationRequested)
                        {
                            Debug.Log("Cancelled connection.");
                            return;
                        }

                        var cancelLenghtStream = GetCancelledStream(ct);
                        var lenghtStreamRead = UniTask.Create(async () => await stream.ReadAsync(MessageLenghtBuffer.AsMemory(0, 4), ct));
                        var (winnerLenght, _, readLenghtResult) = await UniTask.WhenAny(cancelLenghtStream, lenghtStreamRead);
                        if (winnerLenght == 0)
                        {
                            Debug.Log("Cancelled connection.");
                            break;
                        }
                        if (winnerLenght == 1 && readLenghtResult == 0)
                        {
                            Debug.Log("Unity client has been closed.");
                            break;
                        }

                        var length = BitConverter.ToInt32(MessageLenghtBuffer, 0);
                        var data = new byte[length];
                        var cancelStream = GetCancelledStream(ct);
                        var readDataTask = UniTask.Create(async () => await stream.ReadAsync(data.AsMemory(0, length), ct));
                        var (winnerData, _, readDataResult) = await UniTask.WhenAny(cancelStream, readDataTask);
                        if (winnerData == 0)
                        {
                            Debug.Log("Cancelled connection.");
                            break;
                        }
                        if (winnerData == 1 && readDataResult == 0)
                        {
                            Debug.Log("Unity client has been closed.");
                            break;
                        }
                        MessageReceived?.Invoke(data);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        break;
                    }
                }
            }
            finally
            {
                stream.Close();
                client.Close();
                cts = null;
                await stream.DisposeAsync();
                client.Dispose();
                stream = null;
                client = null;
                Disconnected?.Invoke();
            }

        }
        private static async UniTask<int> GetCancelledStream(CancellationToken ct)
        {
            await UniTask.WaitUntilCanceled(ct);
            return 0;
        }

        public static void SendProtocolMessage(ExtensionRequest protocolMessage)
        {
            if (!IsConnected)
                throw new InvalidOperationException("No connection with PlayPad Extension established.");
            var json = JsonUtility.ToJson(protocolMessage);
            Send(json);
        }

        private static void Send(string message)
        {
            var data = Encoding.UTF8.GetBytes(message);
            var length = BitConverter.GetBytes(data.Length);
            stream.Write(length, 0, 4);
            stream.Write(data, 0, data.Length);
        }
    }
}
