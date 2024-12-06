#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.ExternalCommunicators.Authentication;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Extensions;
using ElympicsPlayPad.ExternalCommunicators.Tournament.Extensions;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Responses;
using ElympicsPlayPad.Protocol.WebMessages;
using ElympicsPlayPad.Tournament.Data;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Tournament
{
    public class StandaloneTournamentCommunicator : IExternalTournamentCommunicator, IWebMessageReceiver
    {
        public event Action<TournamentInfo>? TournamentUpdated;
        public TournamentInfo? CurrentTournament => _currentTournamentInfo;

        private TournamentInfo? _currentTournamentInfo;

        private readonly StandaloneExternalTournamentConfig _config;
        private readonly StandaloneExternalAuthenticatorConfig _authConfig;
        private readonly JsCommunicator _jsCommunicator;

        internal StandaloneTournamentCommunicator(StandaloneExternalTournamentConfig config, StandaloneExternalAuthenticatorConfig authConfig, JsCommunicator jsCommunicator)
        {
            _config = config;
            _authConfig = authConfig;
            jsCommunicator.RegisterIWebEventReceiver(this, WebMessageTypes.TournamentUpdated);
        }
        public UniTask<TournamentInfo?> GetTournament(CancellationToken ct = default)
        {
            if (_authConfig.FeatureAccess.HasTournament())
            {
                var dto = new TournamentResponse
                {
                    id = _config.Id,
                    leaderboardCapacity = _config.LeaderboardCapacity,
                    name = _config.TournamentName,
                    ownerId = _config.OwnerId,
                    startDate = _config.StartDate,
                    endDate = _config.EndDate,
                    isDefault = _config.IsDefault,
                };
                _currentTournamentInfo = dto.ToTournamentInfo(_config.PrizePool);
                return UniTask.FromResult(_currentTournamentInfo);
            }
            return UniTask.FromResult<TournamentInfo?>(null);
        }

        public void OnWebMessage(WebMessage message)
        {
            if (string.Equals(message.type, WebMessageTypes.TournamentUpdated) is false)
                throw new Exception($"{nameof(WebGLTournamentCommunicator)} can handle only {WebMessageTypes.TournamentUpdated} event type.");

            var newTournamentData = JsonUtility.FromJson<TournamentUpdatedMessage>(message.message);
            _currentTournamentInfo = newTournamentData.ToTournamentInfo();
            TournamentUpdated?.Invoke(_currentTournamentInfo.Value);
        }
    }
}
