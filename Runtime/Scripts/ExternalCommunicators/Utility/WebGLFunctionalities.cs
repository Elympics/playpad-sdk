using System;
using ElympicsPlayPad.DTO;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.WebMessages.Models;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Utility
{
    internal class WebGLFunctionalities: IDisposable
    {
        private readonly JsCommunicator _jsCommunicator;

        public WebGLFunctionalities(JsCommunicator jsCommunicator)
        {
#if UNITY_WEBGL_API
            WebGLInput.captureAllKeyboardInput = true;
#endif
            _jsCommunicator = jsCommunicator;
            _jsCommunicator.WebObjectReceived += OnWebMessageReceived;

        }
        private static void OnWebMessageReceived(WebMessageObject messageObject)
        {
            switch (messageObject.type)
            {
                case WebMessageTypes.WebGLKeyboardInputControl:
                    OnKeyboardInputControlsRequested(messageObject.message);
                    break;
            }
        }

        private static void OnKeyboardInputControlsRequested(string webMessageMessage)
        {
            var inputControlRequest = JsonUtility.FromJson<WebGLKeyboardInputControlMessage>(webMessageMessage);
#if UNITY_WEBGL_API
            WebGLInput.captureAllKeyboardInput = !inputControlRequest.isKeyboardControlRequested;
#endif
        }
        public void Dispose()
        {
            _jsCommunicator.WebObjectReceived -= OnWebMessageReceived;
        }
    }
}
