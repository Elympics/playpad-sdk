#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.Leaderboard;

namespace ElympicsPlayPad.ExternalCommunicators.Leaderboard
{
    public interface IExternalLeaderboardCommunicator
    {
        public UserHighScoreInfo? UserHighScore { get; }
        public LeaderboardStatusInfo? Leaderboard { get; }
        public event Action<LeaderboardStatusInfo>? LeaderboardUpdated;
        public event Action<UserHighScoreInfo>? UserHighScoreUpdated;
        public UniTask<LeaderboardStatusInfo> FetchLeaderboard(CancellationToken ct = default);
        public UniTask<UserHighScoreInfo?> FetchUserHighScore(CancellationToken ct = default);
    }
}
