using Cysharp.Threading.Tasks;
using Elympics;
using ElympicsPlayPad.Leaderboard;
using ElympicsPlayPad.Protocol.RequestResponse.Leaderboard;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Leaderboard
{
    public abstract class CustomStandaloneLeaderboardCommunicatorBase : MonoBehaviour, IExternalLeaderboardCommunicator
    {
        public abstract UniTask<LeaderboardStatus> FetchLeaderboard(
            string tournamentId,
            string queueName,
            LeaderboardTimeScope timeScope,
            int pageNumber,
            int pageSize,
            LeaderboardRequestType leaderboardRequestType);
        public UniTask<UserHighScore> FetchUserHighScore(string tournamentId, string queueName, LeaderboardTimeScope timeScope, int pageNumber, int pageSize) => throw new System.NotImplementedException();
    }
}
