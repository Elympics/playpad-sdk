#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.DTO;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Utility;
using JetBrains.Annotations;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js
{
    [DefaultExecutionOrder(ElympicsLobbyExecutionOrders.JsCommunicator)]
    internal class JsCommunicator : MonoBehaviour, IJsCommunicatorRetriever
    {
        public event Action<string>? ResponseObjectReceived;
        public event Action<WebMessageObject>? WebObjectReceived;

        private Dictionary<string, List<IWebMessageReceiver>> _webMessageReceivers = new Dictionary<string, List<IWebMessageReceiver>>();

        private int _requestCounter;
        private const string ProtocolVersion = "0.2.0";
        private const string GameObjectName = "JsReceiver";

        private static JsCommunicator instance = null!;
        private JsCommunicationFactory _messageFactory = null!;
        private RequestMessageDispatcher _dispatcher = null!;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                gameObject.name = GameObjectName;
                _messageFactory = new JsCommunicationFactory(ProtocolVersion);
                _dispatcher = new RequestMessageDispatcher(this);
            }
            else
                Destroy(gameObject);
        }

        public async UniTask<TReturn> SendRequestMessage<TInput, TReturn>(string messageType, TInput payload, string address = "")
        {
            var ticket = _requestCounter;
            ++_requestCounter;
            var message = _messageFactory.GenerateRequestMessageJson(ticket, messageType, address, payload);
            Debug.Log($"[{nameof(JsCommunicator)}] Send Request {messageType} message: {message}");
            _dispatcher.RegisterTicket(ticket);
            DispatchHandleMessage(message);
            return await _dispatcher.RequestUniTaskOrThrow<TReturn>(ticket);
        }

        public void SendVoidMessage<TInput>(string messageType, TInput payload)
        {
            var message = _messageFactory.GetVoidMessageJson(messageType, payload);

            if (messageType is not VoidEventTypes.Debug)
                Debug.Log($"[{nameof(JsCommunicator)}] Send Void {messageType} message: {message}");

            DispatchVoidMessage(message);
        }

        public void RegisterIWebEventReceiver(IWebMessageReceiver receiver, params string[] messageTypes)
        {
            foreach (var messageType in messageTypes)
                if (_webMessageReceivers.TryGetValue(messageType, out var list))
                {
                    Debug.Log($"{receiver.GetType()} is registered to {messageType} Web Message.");
                    list.Add(receiver);
                }
                else
                {
                    Debug.Log($"{receiver.GetType()} is added to listeners for {messageType} Web Message.");
                    _webMessageReceivers.Add(messageType,
                        new List<IWebMessageReceiver>()
                        {
                            receiver
                        });
                }
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
                Debug.Log($"[{nameof(JsCommunicator)}] Received Web Event {messageObject}.");
                var message = JsonUtility.FromJson<WebMessageObject>(messageObject);
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
            Debug.Log($"[JS]: Handle Message {json}");
#else
			DispatchMessage(ReactHandlers.HandleMessage, json);
#endif

        }

        private static void DispatchVoidMessage(string json)
        {
#if UNITY_EDITOR || !UNITY_WEBGL
            Debug.Log($"[JS]: Void Message {json}");
#else
			DispatchMessage(ReactHandlers.VoidMessage, json);
#endif

        }
    }
}
