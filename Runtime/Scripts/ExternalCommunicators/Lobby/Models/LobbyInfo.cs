#nullable enable
using ElympicsPlayPad.Rooms;

namespace ElympicsPlayPad.ExternalCommunicators.Lobby.Models
{
    public struct LobbyInfo
    {
        public bool IsMatchReady;
        internal RoomMatchData? MatchData;
    }
}
