using System;
using Elympics.Models.Authentication;

namespace ElympicsPlayPad.ExternalCommunicators.Authentication.Utility
{
    public static class AuthTypeRawUtility
    {
        public const string AuthTypeClaim = "auth-type";
        public const string AuthTypeClaimUnity = "authType";
        public const string EthAddressJwtClaim = "eth-address";
        public const string EthAddressJwtClaimUnity = "ethAddress";
        private const string ClientSecret = "client-secret";
        private const string EthAddress = "eth-address";
        private const string TelegramAuth = "telegram-auth";

        public static AuthType ConvertToAuthType(string authTypeRaw) => authTypeRaw switch
        {
            ClientSecret => AuthType.ClientSecret,
            EthAddress => AuthType.EthAddress,
            TelegramAuth => AuthType.Telegram,
            _ => throw new ArgumentOutOfRangeException(nameof(authTypeRaw), authTypeRaw, null)
        };

        public static string ToUnityNaming(string jsonObject) => jsonObject.Replace($"\"{EthAddressJwtClaim}\":", $"\"{EthAddressJwtClaimUnity}\":").Replace(AuthTypeClaim, AuthTypeClaimUnity);
    }
}
