#nullable enable
using System;
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
        public UniTask<LeaderboardStatusInfo> FetchLeaderboard();
        public UniTask<UserHighScoreInfo?> FetchUserHighScore();
    }
}
