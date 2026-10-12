namespace ElympicsPlayPad.Lobby
{
    public struct DisconnectionData
    {
        public readonly DisconnectionReason Reason;
        public DisconnectionData(DisconnectionReason reason) => Reason = reason;
    }

    public enum DisconnectionReason
    {
        Unknown = 0,
        /// <summary>Signing out, or reconnecting to another region.</summary>
        ClientRequest = 1,
        /// <summary>The lobby client was destroyed.</summary>
        ApplicationShutdown = 2,
        Error = 3,
        Closed = 4,
        /// <summary>Disconnected to reconnect automatically.</summary>
        Reconnection = 5,
        Timeout = 6,
    }
}
