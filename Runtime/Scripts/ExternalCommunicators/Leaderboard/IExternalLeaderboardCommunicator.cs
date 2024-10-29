#nullable enable
using Cysharp.Threading.Tasks;
using Elympics;
using ElympicsPlayPad.Leaderboard;
using ElympicsPlayPad.Protocol.RequestResponse.Leaderboard;

namespace ElympicsPlayPad.ExternalCommunicators.Leaderboard
{
    public interface IExternalLeaderboardCommunicator
    {
        public UniTask<LeaderboardStatus> FetchLeaderboard(string? tournamentId, string? queueName, LeaderboardTimeScope timeScope, int pageNumber, int pageSize, LeaderboardRequestType leaderboardRequestType);
        public UniTask<UserHighScore> FetchUserHighScore(string? tournamentId, string? queueName, LeaderboardTimeScope timeScope, int pageNumber, int pageSize);
    }
}
