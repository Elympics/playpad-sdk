using System;

namespace ElympicsPlayPad.Lobby
{
    /// <summary>Not readonly, so tests can shorten them.</summary>
    internal static class LobbyTimeout
    {
        public static TimeSpan RoomStateChangeConfirmationTimeout = TimeSpan.FromSeconds(15);
        public static TimeSpan ForceMatchmakingCancellationTimeout = TimeSpan.FromSeconds(10);
        public static TimeSpan WebSocketOpeningTimeout = TimeSpan.FromSeconds(10);
        public static TimeSpan WebSocketOperationTimeout = TimeSpan.FromSeconds(10);
        public static TimeSpan WebSocketHeartbeatTimeout = TimeSpan.FromSeconds(30);
        public static TimeSpan WebSocketHeartbeatCheckInterval = TimeSpan.FromSeconds(1);
    }
}
