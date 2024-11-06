using System;

namespace ElympicsPlayPad.Protocol.Requests
{
    [Serializable]
    public struct HandshakeRequest
    {
        public string gameId;
        public string gameName;
        public string versionName;
        public string sdkVersion;
        public string lobbyPackageVersion;
    }
}
