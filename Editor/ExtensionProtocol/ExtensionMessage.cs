using System;
namespace ElympicsPlayPad.Editor.ExtensionProtocol
{
    [Serializable]
    public struct ToExtensionMessage
    {
        public string type;
        public string message;
    }

    [Serializable]
    public struct FromExtensionMessage
    {
        public string type;
        public string response;
        public string error;
    }
}
