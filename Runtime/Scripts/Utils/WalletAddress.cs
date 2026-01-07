#nullable enable
using Elympics.Models.Authentication;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Extensions;
using ElympicsPlayPad.JWT;
using ElympicsPlayPad.JWT.Extensions;

namespace ElympicsPlayPad.Utils
{
    public static class WalletAddress
    {
        public static (string? accountWallet, string? signWallet, string? tonWallet) ExtractWalletAddresses(AuthData authData)
        {
            var jwtPayload = authData.JwtToken.ExtractUnityPayloadFromJwt();
            var (accountWallet, signWallet, tonWallet) = GetAccountAndSignWalletAddressesFromPayload(jwtPayload, authData.AuthType);
            return (accountWallet, signWallet, tonWallet);
        }

        private static (string? accountWallet, string? signWallet, string? tonWallet) GetAccountAndSignWalletAddressesFromPayload(JwtPayload payload, AuthType currentAuthType)
        {
            var accountWallet = payload.ethAddress;
            string? signWallet = null;
            if (currentAuthType.IsWallet())
                signWallet = accountWallet;
            var tonAddress = payload.tonNoBounceAddress ?? payload.tonAddress;
            return (accountWallet, signWallet, tonAddress);
        }
    }
}
