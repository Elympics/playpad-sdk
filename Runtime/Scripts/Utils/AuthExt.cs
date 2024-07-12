using Elympics.Models.Authentication;
namespace ElympicsLobbyPackage.Utils
{
    public static class AuthExt
    {
        public static bool IsAuthorized(this AuthType authType) => authType is not (AuthType.None or AuthType.ClientSecret);

        public static bool IsWallet(this AuthType authType) => authType is (AuthType.EthAddress /*or TON*/);

    }
}