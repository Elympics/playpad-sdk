#nullable enable
using System;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.Leaderboard;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Leaderboard
{
    public abstract class CustomStandaloneLeaderboardCommunicatorBase : MonoBehaviour, IExternalLeaderboardCommunicator
    {
        public abstract UserHighScoreInfo? UserHighScore { get; }
        public abstract LeaderboardStatusInfo? Leaderboard { get; }
        public abstract event Action<LeaderboardStatusInfo>? LeaderboardUpdated;
        public abstract event Action<UserHighScoreInfo>? UserHighScoreUpdated;
        public abstract UniTask<LeaderboardStatusInfo> FetchLeaderboard();
        public abstract UniTask<UserHighScoreInfo?> FetchUserHighScore();
    }
}
