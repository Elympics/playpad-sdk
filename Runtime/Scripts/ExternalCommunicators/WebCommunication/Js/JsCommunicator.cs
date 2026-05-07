#nullable enable
using System;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.ElympicsSystems.Internal;
using ElympicsPlayPad.Utility;
using JetBrains.Annotations;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js
{
    [DefaultExecutionOrder(ElympicsLobbyExecutionOrders.JsCommunicator)]
    internal class JsCommunicator : MonoBehaviour, IPlayPadCommunicator
    {
        public event Action<string>? ResponseMessageReceived;
        public event Action<string>? WebMessageReceived;
        public event Action<string>? WebRequestMessageReceived;

        private const string GameObjectName = "JsReceiver";
        private readonly ElympicsLoggerContext _loggerContext = ElympicsLogger.CurrentContext.WithContext(nameof(JsCommunicator)); // TODO: implement later ~kpieta 2025-01-29

        private void Awake() => gameObject.name = GameObjectName;


        [UsedImplicitly]
        public void HandleResponse(string responseObject)
        {
            Debug.Log($"From JS: handle response {responseObject}");
            ResponseMessageReceived?.Invoke(responseObject);
        }

        [UsedImplicitly]
        public void HandleWebEvent(string messageObject)
        {
            Debug.Log($"From JS: handle web event {messageObject}");
            WebMessageReceived?.Invoke(messageObject);
        }

        public void HandleWebRequest(string messageObject)
        {
            Debug.Log($"From JS: handle web request {messageObject}");
            WebMessageReceived?.Invoke(messageObject);
        }

        public void SendRequestMessage(string messageType, string jsonMessage) => DispatchMessage(messageType, jsonMessage);
        public UniTask Connect() => UniTask.CompletedTask;

        [UsedImplicitly]
        [DllImport("__Internal")]
        public static extern void DispatchMessage(string eventName, string json);
        public void Dispose()
        { }
    }
}
