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
        public readonly string? AccountWallet;
        public readonly string? SignWallet;
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
