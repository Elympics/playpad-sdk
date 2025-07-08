#nullable enable
#pragma warning disable 67 //An event was declared but never used in the class in which it was declared.
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.Leaderboard;
using ElympicsPlayPad.Leaderboard.Extensions;
using ElympicsPlayPad.Protocol.Responses;

namespace ElympicsPlayPad.ExternalCommunicators.Leaderboard
{
    public class StandaloneLeaderboardCommunicator : IExternalLeaderboardCommunicator
    {
        public event Action<LeaderboardStatusInfo>? LeaderboardUpdated;
        public event Action<UserHighScoreInfo>? UserHighScoreUpdated;
        public UserHighScoreInfo? UserHighScore { get; private set; }
        public LeaderboardStatusInfo? Leaderboard { get; private set; }

        public UniTask<LeaderboardStatusInfo> FetchLeaderboard(CancellationToken ct = default)
        {
            Leaderboard = new LeaderboardResponse()
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
            return UniTask.FromResult(Leaderboard.Value);
        }
        public UniTask<UserHighScoreInfo?> FetchUserHighScore(CancellationToken ct = default)
        {
            UserHighScore = new UserHighScoreInfo()
            {
                Points = 99,
                ScoredAt = DateTime.Now - TimeSpan.FromDays(1)
            };
            return UniTask.FromResult(UserHighScore);
        }
    }
}
