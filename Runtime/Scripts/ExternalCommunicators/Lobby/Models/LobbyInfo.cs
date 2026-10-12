#nullable enable
using ElympicsPlayPad.Wrappers;

namespace ElympicsPlayPad.ExternalCommunicators.Lobby.Models
{
    public struct LobbyInfo
    {
        public bool IsMatchReady;
        internal FoundMatch? MatchData;
    }
}
