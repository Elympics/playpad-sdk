#nullable enable
using System;

namespace ElympicsPlayPad.JWT
{
    [Serializable]
    public class JwtPayload
    {
        public string? authType;
        public string? ethAddress;
    }
}
