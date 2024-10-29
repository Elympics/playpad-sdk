using System;
using Cysharp.Threading.Tasks;
using Elympics;
using ElympicsPlayPad.Leaderboard;
using ElympicsPlayPad.Leaderboard.Extensions;
using ElympicsPlayPad.Protocol.RequestResponse.Leaderboard;
using LeaderboardResponse = ElympicsPlayPad.Protocol.RequestResponse.Leaderboard.LeaderboardResponse;

namespace ElympicsPlayPad.ExternalCommunicators.Leaderboard
{
    public class StandaloneLeaderboardCommunicator : IExternalLeaderboardCommunicator
    {
        UniTask<LeaderboardStatus> IExternalLeaderboardCommunicator.FetchLeaderboard(
            string tournamentId,
            string queueName,
            LeaderboardTimeScope timeScope,
            int pageNumber,
            int pageSize,
            LeaderboardRequestType leaderboardRequestType) => UniTask.FromResult(new LeaderboardResponse()
        {
            entries = new Entry[]
            {
                new Entry
                {
                    userId = "00000000-0000-0000-0000-000000000001",
                    nickname = "TestNickName",
                    position = 1,
                    points = 10,
                    scoredAt = DateTime.UtcNow.ToString("o"),
                    matchId = "00000000-0000-0000-0001-000000000000",
                    tournamentId = tournamentId
                }
            },
            totalRecords = 10,
            pageNumber = pageNumber
        }.MapToLeaderboardStatus());
        public UniTask<UserHighScore> FetchUserHighScore(
            string tournamentId,
            string queueName,
            LeaderboardTimeScope timeScope,
            int pageNumber,
            int pageSize) => UniTask.FromResult(new UserHighScore
        {
            Score = 999,
            EndedAt = DateTimeOffset.Now
        });
    }
}
