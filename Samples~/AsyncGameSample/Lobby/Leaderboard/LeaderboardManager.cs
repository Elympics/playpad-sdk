using UnityEngine;
using Elympics;
using System;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    public class LeaderboardManager : MonoBehaviour
    {
        [SerializeField] private LeaderboardDisplay leaderboardDisplay;
        private LeaderboardClient leaderboardClient;

        public void Initialize(string leaderBoardQueue)
        {
            var pageSize = 5;
            var gameVersion = LeaderboardGameVersion.All;
            var leaderboardType = LeaderboardType.BestResult;

            var now = DateTimeOffset.UtcNow;
            var todayStart = now - now.TimeOfDay;
            var timeScopeObject = new LeaderboardTimeScope(todayStart, TimeSpan.FromDays(1));

            leaderboardClient = new LeaderboardClient(pageSize, timeScopeObject, leaderBoardQueue, gameVersion, leaderboardType);
        }

        public void UpdateLeaderboard() => leaderboardClient.FetchFirstPage(leaderboardDisplay.DisplayTop5Entries);

    }
}
