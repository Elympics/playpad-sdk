namespace ElympicsPlayPad.Utility
{
    internal static class ElympicsLobbyExecutionOrders
    {
        public const int JsCommunicator = -10050;
        /// <summary>Before <see cref="ExternalCommunicator"/>, which uses the lobby wrapper in its Awake.</summary>
        public const int LobbyWrapper = -10025;
        public const int ExternalCommunicator = -10000;
        public const int SessionManager = -9975;
        public const int DefaultSmartContractServiceWrapper = -9960;
    }
}
