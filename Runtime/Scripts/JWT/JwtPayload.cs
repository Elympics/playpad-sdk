#nullable enable
using System;

namespace ElympicsPlayPad.JWT
{
    [Serializable]
    public class JwtPayload
    {
        public string? authType;
        public string? ethAddress;
        public string? tonAddress;

        public static string AuthTypeKey => nameof(authType);
        public static string EthAddressKey => nameof(ethAddress);
        public static string TonAddressKey => nameof(tonAddress);
    }
}
