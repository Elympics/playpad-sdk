using System;

namespace ElympicsPlayPad.Auth.Models
{
    [Serializable]
    internal class JwtTokenPayload
    {
        public string nameId;
        public string authType;
        public long nbf;
        public long exp;
        public long iat;
    }
}
