namespace ElympicsPlayPad.Protocol
{
    public static class RequestResponseMessageTypes
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
        public const string GetVirtualDeposit = "GetVirtualDeposit";
        public const string EnsureVirtualDeposit = "EnsureVirtualDeposit";
        public const string ChangeRegion = "ChangeRegion";
        public const string MintNft = "MintNft";
        public const string GetRollTournamentFees = "GetRollTournamentFees";
        public const string GetAvailableCoins = "GetAvailableCurrencies";
        public const string GetWalletCurrencyBalance = "GetWalletCurrencyBalance";
        public const string GetRollingTournamentHistory = "GetRollingTournamentHistory";
        public const string GetUnreadSettlements = "GetUnreadSettlements";
        public const string SignProofOfEntry = "SignProofOfEntry";
        public const string SetActiveTournament = "SetActiveTournament";
        public const string GetRollingTournamentDetails = "GetRollingTournamentDetails";
        public const string ShowOnRamp = "ShowOnRamp";
        public const string SendRawTransaction = "SendRawTransaction";
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
        public const string SnapshotReplay = "SnapshotReplay";
        public const string VirtualDepositUpdated = "VirtualDepositUpdated";
    }

    public static class VoidMessageTypes
    {
        public const string HideSplashScreen = "HideSplashScreen";
        public const string SystemInfoData = "SystemInfoData";
        public const string ElympicsStateUpdated = "ElympicsStateUpdated";
        public const string BreadcrumbMessage = "BreadcrumbMessage";
        public const string Debug = "Debug";
        public const string NetworkStatusMessage = "NetworkStatusMessage";
        public const string OpenUrlMessage = "OpenUrlMessage";
        public const string HeartbeatMessage = "HeartbeatMessage";
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
