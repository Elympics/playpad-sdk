#nullable enable
using Elympics.Models.Authentication;
using ElympicsLobbyPackage.Authorization;
using JetBrains.Annotations;

namespace ElympicsLobbyPackage.Session
{
    [PublicAPI]
    public readonly struct SessionInfo
    {
        public readonly AuthData AuthData;
        public readonly string? AccountWallet;
        public readonly string? SignWallet;
        public readonly Capabilities Capabilities;
        public readonly string Environment;

        public SessionInfo(AuthData authData, string? accountWallet, string? signWallet, Capabilities capabilities, string environment)
        {
            AuthData = authData;
            AccountWallet = accountWallet;
            SignWallet = signWallet;
            Capabilities = capabilities;
            Environment = environment;
        }

        public bool IsAuthorized() => AuthData.AuthType is not (AuthType.ClientSecret or AuthType.None);

        public bool IsWallet() => AuthData.AuthType is (AuthType.EthAddress /*or TON */);
    }
}