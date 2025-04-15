using System;
using Elympics.Models.Authentication;
using ElympicsPlayPad.JWT;

namespace ElympicsPlayPad.ExternalCommunicators.Authentication.Utility
{
    public static class AuthTypeRawUtility
    {
        private const string AuthTypeClaim = "auth-type";
        private const string EthAddressJwtClaim = "eth-address";
        private const string TonAddressJwtClaim = "ton-address";
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

        public static string ToUnityNaming(string jsonObject) => jsonObject.Replace($"\"{EthAddressJwtClaim}\":", $"\"{JwtPayload.EthAddressKey}\":").Replace(AuthTypeClaim, JwtPayload.AuthTypeKey).Replace($"\"{TonAddressJwtClaim}\":", $"\"{JwtPayload.TonAddressKey}\":");
    }
}
