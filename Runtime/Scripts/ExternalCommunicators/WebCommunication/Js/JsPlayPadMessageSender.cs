using System;
using System.Runtime.InteropServices;
using ElympicsPlayPad.Protocol;
using JetBrains.Annotations;

namespace ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js
{
    public static class JsPlayPadMessageSender
    {
        private static void SendMessage(string jsonMessage, string messageType)
        {
            switch (messageType)
            {

                case PlayPadHandlers.HandleMessage:
                    DispatchMessage(PlayPadHandlers.HandleMessage, jsonMessage);
                    break;
                case PlayPadHandlers.VoidMessage:
                    DispatchMessage(PlayPadHandlers.VoidMessage, jsonMessage);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null);
            }
        }

        [UsedImplicitly]
        [DllImport("__Internal")]
        public static extern void DispatchMessage(string eventName, string json);
    }
}
