using System;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.WebMessages;
#if !UNITY_EDITOR && UNITY_WEBGL_API
using UnityEngine;
#endif

namespace ElympicsPlayPad.ExternalCommunicators.Utility
{
    internal class WebGLFunctionalities : IDisposable, IWebMessageReceiver
    {
        private readonly PlayPadMessagingSystem _playPadMessagingSystem;

        public WebGLFunctionalities(PlayPadMessagingSystem playPadMessagingSystem)
        {
#if !UNITY_EDITOR && UNITY_WEBGL_API
            WebGLInput.captureAllKeyboardInput = true;
            _playPadMessagingSystem = playPadMessagingSystem;
            _playPadMessagingSystem.RegisterIWebEventReceiver(this, WebMessageTypes.WebGLKeyboardInputControl);
#endif

        }

        private static void OnKeyboardInputControlsRequested(string webMessageMessage)
        {
#if!UNITY_EDITOR && UNITY_WEBGL_API
            var inputControlRequest = JsonUtility.FromJson<WebGLKeyboardInputControlMessage>(webMessageMessage);
            WebGLInput.captureAllKeyboardInput = !inputControlRequest.isKeyboardControlRequested;
#endif
        }
        public void Dispose()
        {
#if !UNITY_EDITOR && UNITY_WEBGL_API
            _playPadMessagingSystem.WebObjectReceived -= OnWebMessage;
#endif
        }
        public void OnWebMessage(WebMessage message)
        {
            switch (message.type)
            {
                case WebMessageTypes.WebGLKeyboardInputControl:
                    OnKeyboardInputControlsRequested(message.message);
                    break;
                default:
                    break;
            }
        }
    }
}
