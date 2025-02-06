namespace ElympicsPlayPad.Protocol
{
    public static class ReturnEventTypes
    {
        public const string SignTypedData = "SignTypedData";
        public const string EncodeFunctionData = "EncodeFunctionData";
        public const string GetValue = "GetValue";
        public const string SendTransaction = "SendTransaction";
        public const string Handshake = "Handshake";
        public const string GetAuthentication = "GetAuthentication";
        public const string GetLeaderboard = "GetLeaderboard";
        public const string GetUserHighScore = "GetUserHighScore";
        public const string GetTournament = "GetTournament";
        public const string GetPlayStatus = "GetPlayStatus";
        public const string ShowPlayPadModal = "ShowPlayPadModal";
    }

    public static class WebMessageTypes
    {
        public const string WebGLKeyboardInputControl = "WebGLKeyboardInputControl";
        public const string AuthenticationUpdated = "AuthenticationUpdated";
        public const string TournamentUpdated = "TournamentUpdated";
        public const string LeaderboardUpdated = "LeaderboardUpdated";
        public const string UserHighScoreUpdated = "UserHighScoreUpdated";
        public const string PlayStatusUpdated = "PlayStatusUpdated";
        public const string RegionUpdated = "RegionUpdated";
    }

    public static class VoidEventTypes
    {
        public const string HideSplashScreen = "HideSplashScreen";
        public const string SystemInfoData = "SystemInfoData";
        public const string ElympicsStateUpdated = "ElympicsStateUpdated";
        public const string BreadcrumbMessage = "BreadcrumbMessage";
        public const string Debug = "Debug";
    }

    public static class DebugMessageTypes
    {
        public const string RTT = "RTT";
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    public static class PlayPadHandlers
    {
        public const string HandleMessage = "HandleMessage";
        public const string VoidMessage = "HandleVoidMessage";
    }
#endif
}
