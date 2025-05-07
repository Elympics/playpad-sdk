#nullable enable
using System;
using System.Runtime.InteropServices;
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

        private const string GameObjectName = "JsReceiver";
        private ElympicsLoggerContext _loggerContext; //TODO implement later k.pieta 29.01.2025

        private void Awake() => gameObject.name = GameObjectName;


        [UsedImplicitly]
        public void HandleResponse(string responseObject) => ResponseMessageReceived?.Invoke(responseObject);

        [UsedImplicitly]
        public void HandleWebEvent(string messageObject) => WebMessageReceived?.Invoke(messageObject);

        public void SendRequestMessage(string messageType, string jsonMessage) => DispatchMessage(messageType, jsonMessage);

        [UsedImplicitly]
        [DllImport("__Internal")]
        public static extern void DispatchMessage(string eventName, string json);
        public void Dispose()
        { }
    }
}
