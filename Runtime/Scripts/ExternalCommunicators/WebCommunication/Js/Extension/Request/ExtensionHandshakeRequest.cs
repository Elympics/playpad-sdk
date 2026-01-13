using System;

namespace ElympicsPlayPad.Editor.ExtensionProtocol
{
    [Serializable]
    internal struct ExtensionHandshakeRequest
    {
        public string jwt;
        public string gameId;
        public string gameVersion;
        public string gameName;
        public string playpadSdkVersion;
        public string extensionProtocolVersion;
        public string playpadProtocolVersion;
    }
}
