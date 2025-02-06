using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics.ElympicsSystems.Internal;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Leaderboard;
using ElympicsPlayPad.Leaderboard.Extensions;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Responses;
using ElympicsPlayPad.Protocol.WebMessages;
using UnityEngine;

#nullable enable
namespace ElympicsPlayPad.ExternalCommunicators.Leaderboard
{
    internal class WebGLLeaderboardCommunicator : IExternalLeaderboardCommunicator, IWebMessageReceiver
    {
        public UserHighScoreInfo? UserHighScore => _userHighScore;
        public LeaderboardStatusInfo? Leaderboard => _leaderboard;

        private UserHighScoreInfo? _userHighScore;
        private LeaderboardStatusInfo? _leaderboard;
        public event Action<LeaderboardStatusInfo>? LeaderboardUpdated;
        public event Action<UserHighScoreInfo>? UserHighScoreUpdated;
        private ElympicsLoggerContext _logger;

        private readonly JsCommunicator _jsCommunicator;
        public WebGLLeaderboardCommunicator(JsCommunicator jsCommunicator, ElympicsLoggerContext logger)
        {
            _jsCommunicator = jsCommunicator;
            _jsCommunicator.RegisterIWebEventReceiver(this, WebMessageTypes.LeaderboardUpdated, WebMessageTypes.UserHighScoreUpdated);
            _logger = logger.WithContext(nameof(WebGLLeaderboardCommunicator));
        }

        public async UniTask<LeaderboardStatusInfo> FetchLeaderboard(CancellationToken ct = default)
        {
            var result = await _jsCommunicator.SendRequestMessage<EmptyPayload, LeaderboardResponse>(ReturnEventTypes.GetLeaderboard, default, ct);
            _leaderboard = result.MapToLeaderboardStatus();
            return _leaderboard.Value;
        }
        public async UniTask<UserHighScoreInfo?> FetchUserHighScore(CancellationToken ct = default)
        {
            var response = await _jsCommunicator.SendRequestMessage<EmptyPayload, UserHighScoreResponse>(ReturnEventTypes.GetUserHighScore, default, ct);
            _userHighScore = response.MapToUserHighScore();
            return _userHighScore;
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
                        _leaderboard = leaderboardUpdate.MapToLeaderboardStatus();
                        LeaderboardUpdated?.Invoke(_leaderboard.Value);
                        break;
                    }
                    case WebMessageTypes.UserHighScoreUpdated:
                    {
                        var highScoreUpdate = JsonUtility.FromJson<UserHighScoreUpdatedMessage>(message.message);
                        _userHighScore = highScoreUpdate.MapToUserHighScore();
                        if (_userHighScore.HasValue is false)
                            return;

                        UserHighScoreUpdated?.Invoke(_userHighScore.Value);
                        break;
                    }
                    default:
                        logger.Error($"Unable to handle message {message.type}");
                        break;
                }
            }
            catch (Exception e)
            {
                throw logger.CaptureAndThrow(e);
            }
        }
    }
}
