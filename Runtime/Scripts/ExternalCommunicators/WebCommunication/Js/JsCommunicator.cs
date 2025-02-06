#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.ElympicsSystems.Internal;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Responses;
using ElympicsPlayPad.Protocol.WebMessages;
using ElympicsPlayPad.Utility;
using JetBrains.Annotations;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js
{
    [DefaultExecutionOrder(ElympicsLobbyExecutionOrders.JsCommunicator)]
    internal class JsCommunicator : MonoBehaviour, IJsCommunicatorRetriever
    {
        public event Action<string>? ResponseObjectReceived;
        public event Action<WebMessage>? WebObjectReceived;

        private Dictionary<string, List<IWebMessageReceiver>> _webMessageReceivers = new Dictionary<string, List<IWebMessageReceiver>>();

        private int _requestCounter;
        internal const string ProtocolVersion = "0.2.1";
        private const string GameObjectName = "JsReceiver";

        private static JsCommunicator instance = null!;
        private JsCommunicationFactory _messageFactory = null!;
        private RequestMessageDispatcher _dispatcher = null!;

        private ElympicsLoggerContext _loggerContext; //TODO implement later k.pieta 29.01.2025

        public void Init(ElympicsLoggerContext loggerContext)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            _loggerContext = loggerContext.WithContext(nameof(JsCommunicator));
            gameObject.name = GameObjectName;
            _messageFactory = new JsCommunicationFactory();
            _dispatcher = new RequestMessageDispatcher(this, _loggerContext);
        }

        public async UniTask<TReturn> SendRequestMessage<TInput, TReturn>(string messageType, TInput? payload, CancellationToken ct)
            where TInput : struct
            where TReturn : struct
        {
            var ticket = _requestCounter;
            ++_requestCounter;
            var message = _messageFactory.GenerateRequestMessageJson(ticket, messageType, payload);
            ElympicsLogger.Log($"Send Request {messageType} message: {message}");
            _dispatcher.RegisterTicket(ticket);
            DispatchHandleMessage(message);
            return await _dispatcher.RequestUniTaskOrThrow<TReturn>(ticket, ct);
        }

        public void SendDebugMessage<TInput>(string debugType, TInput? payload = null)
            where TInput : struct
        {
            var debug = _messageFactory.GetDebugMessageJson(debugType, payload);
            var voidMessage = new StringPayloadResponse()
            {
                message = debug
            };
            var messageToDispatch = _messageFactory.GetVoidMessageJson<StringPayloadResponse>(VoidEventTypes.Debug, voidMessage);
            DispatchVoidMessage(messageToDispatch);
        }

        public void SendVoidMessage<TInput>(string messageType, TInput? payload = null)
            where TInput : struct
        {
            var message = _messageFactory.GetVoidMessageJson(messageType, payload);
            if (BlockEventLog(messageType) is false)
                ElympicsLogger.Log($"Send Void {messageType} message: {message}");

            DispatchVoidMessage(message);

            return;

            bool BlockEventLog(string type) => type.Equals(VoidEventTypes.BreadcrumbMessage);
        }


        public void RegisterIWebEventReceiver(IWebMessageReceiver receiver, params string[] messageTypes)
        {
            foreach (var messageType in messageTypes)
                if (_webMessageReceivers.TryGetValue(messageType, out var list))
                    list.Add(receiver);
                else
                    _webMessageReceivers.Add(messageType,
                        new List<IWebMessageReceiver>()
                        {
                            receiver
                        });
        }

        [UsedImplicitly]
        public void HandleResponse(string responseObject)
        {
            try
            {
                ResponseObjectReceived?.Invoke(responseObject);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [UsedImplicitly]
        public void HandleWebEvent(string messageObject)
        {
            try
            {
                ElympicsLogger.Log($"Received WebMessage message: {messageObject}");
                var message = JsonUtility.FromJson<WebMessage>(messageObject);
                WebObjectReceived?.Invoke(message);
                if (_webMessageReceivers.TryGetValue(message.type, out var listeners))
                    listeners?.ForEach(x => x?.OnWebMessage(message));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

        }

        [UsedImplicitly]
        [DllImport("__Internal")]
        public static extern void DispatchMessage(string eventName, string json);

        private static void DispatchHandleMessage(string json)
        {
#if UNITY_EDITOR || !UNITY_WEBGL
            Debug.Log($"[{nameof(JsCommunicator)}]: Handle Message {json}");
#else
			DispatchMessage(PlayPadHandlers.HandleMessage, json);
#endif

        }

        private static void DispatchVoidMessage(string json)
        {
#if UNITY_EDITOR || !UNITY_WEBGL
            Debug.Log($"[{nameof(JsCommunicator)}]: Void Message {json}");
#else
			DispatchMessage(PlayPadHandlers.VoidMessage, json);
#endif

        }
    }
}
