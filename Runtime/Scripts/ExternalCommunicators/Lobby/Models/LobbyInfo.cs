using Elympics.Models.Matchmaking;
namespace ElympicsPlayPad.ExternalCommunicators.Lobby.Models
{
    public struct LobbyInfo
    {
        public bool IsMatchReady;
        internal MatchmakingFinishedData MatchData;
    }
}
