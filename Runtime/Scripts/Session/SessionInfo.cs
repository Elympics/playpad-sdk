#nullable enable
using Elympics.Models.Authentication;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;
using JetBrains.Annotations;

namespace ElympicsPlayPad.Session
{
    [PublicAPI]
    public readonly struct SessionInfo
    {
        public readonly AuthData AuthData;
        /// <summary>EVM wallet address or null if current user doesn't have an EVM wallet connected.</summary>
        public readonly string? AccountWallet;
        /// <summary>
        /// If <see cref="AuthData"/>.<see cref="AuthData.AuthType"/> is <see cref="AuthType.EthAddress"/>,
        /// this will field has the same value as <see cref="AccountWallet"/>, otherwise it is null.
        /// </summary>
        public readonly string? SignWallet;
        /// <summary>TON wallet address or null if current user doesn't have a TON wallet connected.</summary>
        public readonly string? TonWalletAddress;
        public readonly Capabilities Capabilities;
        public readonly FeatureAccess Features;
        public readonly string Environment;
        public readonly bool IsMobile;
        public readonly string ClosestRegion;

        public SessionInfo(
            AuthData authData,
            string? accountWallet,
            string? signWallet,
            Capabilities capabilities,
            string environment,
            bool isMobile,
            string closestRegion,
            FeatureAccess features,
            string? tonWalletAddress)
        {
            AuthData = authData;
            AccountWallet = accountWallet;
            SignWallet = signWallet;
            Capabilities = capabilities;
            Environment = environment;
            IsMobile = isMobile;
            ClosestRegion = closestRegion;
            Features = features;
            TonWalletAddress = tonWalletAddress;
        }

        public bool IsAuthorized() => AuthData.AuthType is not (AuthType.ClientSecret or AuthType.None);

        public bool IsWallet() => AuthData.AuthType is AuthType.EthAddress /*or TON */ || !string.IsNullOrEmpty(TonWalletAddress);
    }
}
