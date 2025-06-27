#nullable enable
using System;

namespace ElympicsPlayPad.JWT
{
    [Serializable]
    public class JwtPayload
    {
        public string? authType;
        public string? ethAddress;
        public string? tonAddress; // raw ton address
        public string? tonNoBounceAddress; //no bounce formatted address - preferred.

        public static string AuthTypeKey => nameof(authType);
        public static string EthAddressKey => nameof(ethAddress);
        public static string TonAddressKey => nameof(tonAddress);

        public static string TonNoBounceAddressKey = nameof(tonNoBounceAddress);
    }
}
