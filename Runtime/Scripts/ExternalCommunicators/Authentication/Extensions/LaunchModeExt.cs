using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;

namespace ElympicsPlayPad.ExternalCommunicators.Authentication.Extensions
{
    public static class LaunchModeExt
    {
        public static bool HasNone(this LaunchMode launchMode) => launchMode == LaunchMode.None;

        public static bool HasLobby(this LaunchMode launchMode) => launchMode.HasFlag(LaunchMode.Lobby);
        public static bool HasGamePlay(this LaunchMode launchMode) => launchMode.HasFlag(LaunchMode.Gameplay);

        public static bool HasOnlyLobby(this LaunchMode launchMode) => launchMode == LaunchMode.Lobby;
        public static bool HasOnlyGamePlay(this LaunchMode launchMode) => launchMode == LaunchMode.Gameplay;
    }
}
