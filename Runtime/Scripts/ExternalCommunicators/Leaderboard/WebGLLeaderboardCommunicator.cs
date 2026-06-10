using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics.Core.Logger;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Leaderboard;
using ElympicsPlayPad.Leaderboard.Extensions;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Responses;
using ElympicsPlayPad.Protocol.WebMessages;
using UnityEngine;
using LeaderboardResponse = ElympicsPlayPad.Protocol.Responses.LeaderboardResponse;

#nullable enable
namespace ElympicsPlayPad.ExternalCommunicators.Leaderboard
{
    internal class WebGLLeaderboardCommunicator : IExternalLeaderboardCommunicator, IWebMessageReceiver
    {
        public UserHighScoreInfo? UserHighScore { get; private set; }
        public LeaderboardStatusInfo? Leaderboard { get; private set; }

        public event Action<LeaderboardStatusInfo>? LeaderboardUpdated;
        public event Action<UserHighScoreInfo>? UserHighScoreUpdated;
        private readonly LoggerConfig _logger = ElympicsLogger.WithPlayPadSdkService().WithClass(typeof(WebGLLeaderboardCommunicator));

        private readonly PlayPadMessagingSystem _playPadMessagingSystem;
        public WebGLLeaderboardCommunicator(PlayPadMessagingSystem playPadMessagingSystem)
        {
            _playPadMessagingSystem = playPadMessagingSystem;
            _playPadMessagingSystem.RegisterIWebEventReceiver(this, WebMessageTypes.LeaderboardUpdated, WebMessageTypes.UserHighScoreUpdated);
        }

        public async UniTask<LeaderboardStatusInfo> FetchLeaderboard(CancellationToken ct = default)
        {
            var result = await _playPadMessagingSystem.SendRequestMessage<EmptyPayload, LeaderboardResponse>(RequestResponseMessageTypes.GetLeaderboard, default, ct);
            Leaderboard = result.MapToLeaderboardStatus();
            return Leaderboard.Value;
        }
        public async UniTask<UserHighScoreInfo?> FetchUserHighScore(CancellationToken ct = default)
        {
            var response = await _playPadMessagingSystem.SendRequestMessage<EmptyPayload, UserHighScoreResponse>(RequestResponseMessageTypes.GetUserHighScore, default, ct);
            UserHighScore = response.MapToUserHighScore();
            return UserHighScore;
        }
        public void OnWebMessage(WebMessage message)
        {
            var logger = _logger.WithMethodName();
            try
            {
                switch (message.type)
                {
                    case WebMessageTypes.LeaderboardUpdated:
                    {
                        var leaderboardUpdate = JsonUtility.FromJson<LeaderboardUpdatedMessage>(message.message);
                        Leaderboard = leaderboardUpdate.MapToLeaderboardStatus();
                        LeaderboardUpdated?.Invoke(Leaderboard.Value);
                        break;
                    }
                    case WebMessageTypes.UserHighScoreUpdated:
                    {
                        var highScoreUpdate = JsonUtility.FromJson<UserHighScoreUpdatedMessage>(message.message);
                        UserHighScore = highScoreUpdate.MapToUserHighScore();
                        if (!UserHighScore.HasValue)
                            return;

                        UserHighScoreUpdated?.Invoke(UserHighScore.Value);
                        break;
                    }
                    default:
                        logger.LogError($"Unable to handle message {message.type}");
                        break;
                }
            }
            catch (Exception e)
            {
                throw logger.LogExceptionAndReturn(e);
            }
        }
    }
}
