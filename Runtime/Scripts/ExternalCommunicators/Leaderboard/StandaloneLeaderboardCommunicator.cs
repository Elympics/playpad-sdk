using System;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.Leaderboard;
using ElympicsPlayPad.Leaderboard.Extensions;
using ElympicsPlayPad.Protocol.Responses;

namespace ElympicsPlayPad.ExternalCommunicators.Leaderboard
{
    public class StandaloneLeaderboardCommunicator : IExternalLeaderboardCommunicator
    {
        public UserHighScoreInfo? UserHighScore => _userHighScore;
        public LeaderboardStatusInfo? Leaderboard => _leaderboard;
        private UserHighScoreInfo? _userHighScore;
        private LeaderboardStatusInfo? _leaderboard;
        public event Action<LeaderboardStatusInfo> LeaderboardUpdated;
        public event Action<UserHighScoreInfo> UserHighScoreUpdated;
        UniTask<LeaderboardStatusInfo> IExternalLeaderboardCommunicator.FetchLeaderboard()
        {
            _leaderboard = new LeaderboardResponse()
            {
                entries = new[]
                {
                    new Entry
                    {
                        userId = "00000000-0000-0000-0000-000000000001",
                        nickname = "TestNickName",
                        position = 1,
                        score = 10,
                        scoredAt = DateTime.UtcNow.ToString("o"),
                        matchId = "00000000-0000-0000-0001-000000000000",
                        tournamentId = "abcdef"
                    }
                },
                participants = 10,
            }.MapToLeaderboardStatus();
            return UniTask.FromResult(_leaderboard.Value);
        }

        public UniTask<UserHighScoreInfo?> FetchUserHighScore()
        {
            _userHighScore = new UserHighScoreInfo()
            {
                Points = 99,
                ScoredAt = DateTime.Now - TimeSpan.FromDays(1)
            };
            return UniTask.FromResult(_userHighScore);
        }
    }
}
