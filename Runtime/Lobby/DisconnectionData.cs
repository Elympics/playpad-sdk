namespace ElympicsPlayPad.Lobby
{
    public struct DisconnectionData
    {
        public readonly DisconnectionReason Reason;
        public DisconnectionData(DisconnectionReason reason) => Reason = reason;
    }

    /// <summary>
    /// Specifies the reason for a disconnection in a networked application.
    /// </summary>
    public enum DisconnectionReason
    {
        /// <summary>
        /// The disconnection reason is unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The disconnection was requested by the client by signing out or when reconnecting to a new region.
        /// </summary>
        ClientRequest = 1,

        /// <summary>
        /// When the lobby client is destroyed.
        /// </summary>
        ApplicationShutdown = 2,

        /// <summary>
        /// The disconnection was due to an websocket error.
        /// </summary>
        Error = 3,

        /// <summary>
        /// The websocket connection was closed.
        /// </summary>
        Closed = 4,

        /// <summary>
        /// The disconnection is due to a auto scheduled reconnection attempt.
        /// </summary>
        Reconnection = 5,

        /// <summary>
        /// The disconnection was due to a timeout.
        /// </summary>
        Timeout = 6,
    }
}
