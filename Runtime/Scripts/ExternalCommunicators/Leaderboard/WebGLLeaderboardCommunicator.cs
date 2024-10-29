using System;
using Cysharp.Threading.Tasks;
using Elympics;
using ElympicsPlayPad.DTO;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Leaderboard;
using ElympicsPlayPad.Leaderboard.Extensions;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.RequestResponse.Leaderboard;
using LeaderboardResponse = ElympicsPlayPad.Protocol.RequestResponse.Leaderboard.LeaderboardResponse;

#nullable enable
namespace ElympicsPlayPad.ExternalCommunicators.Leaderboard
{
    internal class WebGLLeaderboardCommunicator : IExternalLeaderboardCommunicator
    {
        private readonly JsCommunicator _jsCommunicator;
        public WebGLLeaderboardCommunicator(JsCommunicator jsCommunicator) => _jsCommunicator = jsCommunicator;

        public async UniTask<LeaderboardStatus> FetchLeaderboard(
            string? tournamentId,
            string? queueName,
            LeaderboardTimeScope timeScope,
            int pageNumber,
            int pageSize,
            LeaderboardRequestType leaderboardRequestType)
        {
            var request = new LeaderboardRequest
            {
                tournamentId = tournamentId,
                queueName = queueName,
                timeScopeType = timeScope.LeaderboardTimeScopeType,
                pageNumber = pageNumber,
                pageSize = pageSize,
                fetchType = "Max",
                dateFrom = timeScope.LeaderboardTimeScopeType == LeaderboardTimeScopeType.Custom ? timeScope.DateFrom.ToString("o") : string.Empty,
                dateTo = timeScope.LeaderboardTimeScopeType == LeaderboardTimeScopeType.Custom ? timeScope.DateTo.ToString("o") : string.Empty,
            };
            var response = leaderboardRequestType switch
            {
                LeaderboardRequestType.Regular => await _jsCommunicator.SendRequestMessage<LeaderboardRequest, LeaderboardResponse>(ReturnEventTypes.GetLeaderboard, request),
                LeaderboardRequestType.UserCentered => await _jsCommunicator.SendRequestMessage<LeaderboardRequest, LeaderboardResponse>(ReturnEventTypes.GetLeaderboardUserCentered, request),
                _ => throw new ArgumentOutOfRangeException(nameof(leaderboardRequestType), leaderboardRequestType, null)
            };
            return response.MapToLeaderboardStatus();
        }
        public async UniTask<UserHighScore> FetchUserHighScore(
            string? tournamentId,
            string? queueName,
            LeaderboardTimeScope timeScope,
            int pageNumber,
            int pageSize)
        {
            var request = new LeaderboardRequest
            {
                tournamentId = tournamentId,
                queueName = queueName,
                timeScopeType = timeScope.LeaderboardTimeScopeType,
                pageNumber = pageNumber,
                pageSize = pageSize,
                fetchType = "Max",
                dateFrom = timeScope.LeaderboardTimeScopeType == LeaderboardTimeScopeType.Custom ? timeScope.DateFrom.ToString("o") : string.Empty,
                dateTo = timeScope.LeaderboardTimeScopeType == LeaderboardTimeScopeType.Custom ? timeScope.DateTo.ToString("o") : string.Empty,
            };
            var response = await _jsCommunicator.SendRequestMessage<LeaderboardRequest, UserHighScoreResponse>(ReturnEventTypes.GetLeaderboardUserHighScore, request);
            return response.MapToUserHighScore();
        }
    }
}
