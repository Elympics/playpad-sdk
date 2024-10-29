namespace ElympicsPlayPad.Protocol
{
    public static class ReturnEventTypes
    {
        public const string SignTypedData = "SignTypedData";
        public const string Connect = "Connect";
        public const string EncodeFunctionData = "EncodeFunctionData";
        public const string GetValue = "GetValue";
        public const string SendTransaction = "SendTransaction";
        public const string Handshake = "Handshake";
        public const string GetTrustState = "GetTrustState";
        public const string GetLeaderboard = "GetLeaderboard";
        public const string GetLeaderboardUserCentered = "GetLeaderboardUserCentered";
        public const string GetLeaderboardUserHighScore = "GetLeaderboardUserHighScore";
        public const string CanPlayTournament = "CanPlayTournament";
    }

    public static class WebMessageTypes
    {
        public const string WebGLKeyboardInputControl = "WebGLKeyboardInputControl";
        public const string AuthDataChanged = "AuthDataChanged";
        public const string TrustTransactionFinished = "TrustTransactionFinished";
        public const string TournamentUpdated = "TournamentUpdated";
    }

    public static class VoidEventTypes
    {
        public const string ShowConnectToWallet = "ShowConnectUI";
        public const string ShowChainSelectionUI = "ShowChainSelectionUI";
        public const string GameFinished = "GameFinished";
        public const string GameplayStarted = "GameplayStarted";
        public const string GameplayFinished = "GameplayFinished";
        public const string ShowAccountUI = "ShowAccountUI";
        public const string ApplicationInitialized = "ApplicationInitialized";
        public const string IncreaseTrust = "IncreaseTrust";
        public const string Debug = "Debug";
    }

    public static class DebugMessageTypes
    {
        public const string RTT = "RTT";
    }

    public static class ReactHandlers
    {
        public const string HandleMessage = "HandleMessage";
        public const string VoidMessage = "HandleVoidMessage";
    }
}
