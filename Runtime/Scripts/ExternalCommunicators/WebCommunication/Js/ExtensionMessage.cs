using System;

namespace ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js
{
    [Serializable]
    public struct ExtensionRequest
    {
        public string type;
        public string message;
    }

    [Serializable]
    public struct ExtensionMessage
    {
        public string type;
        public string response;
        public int errorCode;
        public string errorMessage;
    }
}
