using System;

namespace ElympicsPlayPad.Protocol.WebMessages
{
    [Serializable]
    public struct AuthenticationUpdatedMessage
    {
        public string jwt;
        public string userId;
        public string nickname;
    }
}
