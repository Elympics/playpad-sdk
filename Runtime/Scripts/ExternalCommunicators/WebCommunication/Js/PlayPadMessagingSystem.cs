#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics.Core.Logger;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.WebMessages;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js
{
    internal class PlayPadMessagingSystem : MonoBehaviour, IPlayPadMessagingSystem
    {
        public event Action<WebMessage>? WebObjectReceived;
        internal const string ProtocolVersion = "0.3.0";
        private IPlayPadCommunicator playpadCommunicator = null!;
        private PlayPadMessageFactory _messageFactory = null!;
        private RequestMessageDispatcher _dispatcher = null!;
        private int _requestCounter;
        private readonly Dictionary<string, List<IWebMessageReceiver>> _webMessageReceivers = new();

        public void Init(PlayPadCommunicatorFactory factory)
        {
            _messageFactory = new PlayPadMessageFactory();
            _dispatcher = new RequestMessageDispatcher();
            playpadCommunicator = factory.GetPlayPadCommunicator();
            playpadCommunicator.WebMessageReceived += OnWebMessageReceived;
            playpadCommunicator.ResponseMessageReceived += OnResponseMessageReceived;
        }

        public async UniTask<TReturn> SendRequestMessage<TInput, TReturn>(string messageType, TInput? payload, CancellationToken ct)
            where TInput : struct
            where TReturn : struct
        {
            var ticket = _requestCounter;
            ++_requestCounter;
            var message = _messageFactory.GenerateRequestMessageJson(ticket, messageType, payload);
            ElympicsLogger.LogInfo($"Send Request {messageType} message: {message}");
            _dispatcher.RegisterTicket(ticket);
            playpadCommunicator.SendRequestMessage(PlayPadHandlers.HandleMessage, message);
            return await _dispatcher.RequestUniTaskOrThrow<TReturn>(ticket, ct);
        }

        public void SendVoidMessage<TInput>(string messageType, TInput? payload = null)
            where TInput : struct
        {
            var message = _messageFactory.GetVoidMessageJson(messageType, payload);
            if (!BlockEventLog(messageType))
                ElympicsLogger.LogInfo($"Send Void {messageType} message: {message}");

            playpadCommunicator.SendRequestMessage(PlayPadHandlers.VoidMessage, message);
            return;

            static bool BlockEventLog(string type) =>
                type.Equals(VoidMessageTypes.BreadcrumbMessage) || type.Equals(VoidMessageTypes.NetworkStatusMessage) || type.Equals(VoidMessageTypes.HeartbeatMessage);
        }

        public void RegisterIWebEventReceiver(IWebMessageReceiver receiver, string messageType) => RegisterHandler(receiver, messageType);

        public void RegisterIWebEventReceiver(IWebMessageReceiver receiver, params string[] messageTypes)
        {
            foreach (var messageType in messageTypes)
                RegisterHandler(receiver, messageType);
        }

        public void UnregisterIWebEventReceiver(IWebMessageReceiver receiver, string messageType)
        {
            if (_webMessageReceivers.TryGetValue(messageType, out var list))
                _ = list.Remove(receiver);
        }
        public UniTask Connect() => playpadCommunicator.Connect();
        public void Deinit()
        {
            playpadCommunicator.Dispose();
            playpadCommunicator.WebMessageReceived -= OnWebMessageReceived;
            playpadCommunicator.ResponseMessageReceived -= OnResponseMessageReceived;

        }

        private void OnResponseMessageReceived(string messageJson)
        {
            try
            {
                _dispatcher.OnResponseObjectReceived(messageJson);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        private void OnWebMessageReceived(string jsonMessage)
        {
            try
            {
                ElympicsLogger.LogInfo($"Received WebMessage message: {jsonMessage}");
                var message = JsonUtility.FromJson<WebMessage>(jsonMessage);
                WebObjectReceived?.Invoke(message);
                if (_webMessageReceivers.TryGetValue(message.type, out var listeners))
                    listeners?.ForEach(x => x?.OnWebMessage(message));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void RegisterHandler(IWebMessageReceiver receiver, string messageType)
        {
            if (_webMessageReceivers.TryGetValue(messageType, out var list))
                list.Add(receiver);
            else
                _webMessageReceivers.Add(messageType,
                    new List<IWebMessageReceiver>()
                    {
                        receiver
                    });
        }
    }
}
