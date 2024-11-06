using UnityEngine;
using System;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    public class LeaderboardManager : MonoBehaviour
    {
        [SerializeField] private LeaderboardDisplay leaderboardDisplay;

        public void UpdateLeaderboard() => throw new NotImplementedException(); //leaderboardClient.FetchFirstPage(leaderboardDisplay.DisplayTop5Entries);
    }
}
