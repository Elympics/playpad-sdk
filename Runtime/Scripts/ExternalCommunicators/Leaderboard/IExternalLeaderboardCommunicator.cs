#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.Leaderboard;

namespace ElympicsPlayPad.ExternalCommunicators.Leaderboard
{
    public interface IExternalLeaderboardCommunicator
    {
        UserHighScoreInfo? UserHighScore { get; }
        LeaderboardStatusInfo? Leaderboard { get; }
        event Action<LeaderboardStatusInfo>? LeaderboardUpdated;
        event Action<UserHighScoreInfo>? UserHighScoreUpdated;
        UniTask<LeaderboardStatusInfo> FetchLeaderboard(CancellationToken ct = default);
        UniTask<UserHighScoreInfo?> FetchUserHighScore(CancellationToken ct = default);
    }
}
