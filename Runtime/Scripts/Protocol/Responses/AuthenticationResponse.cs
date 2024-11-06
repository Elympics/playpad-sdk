using System;

namespace ElympicsPlayPad.Protocol.Responses
{
    [Serializable]
    public struct AuthenticationResponse
    {
        public string jwt;
        public string userId;
        public string nickname;
    }
}
