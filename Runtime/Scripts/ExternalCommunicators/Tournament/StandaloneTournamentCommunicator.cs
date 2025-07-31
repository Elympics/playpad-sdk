#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using ElympicsPlayPad.ExternalCommunicators.Authentication;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Extensions;
using ElympicsPlayPad.ExternalCommunicators.Tournament.Extensions;
using ElympicsPlayPad.ExternalCommunicators.Tournament.Models.MyNamespace;
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
        public TournamentInfo? CurrentTournament { get; private set; }

        private readonly StandaloneExternalTournamentConfig _config;
        private readonly StandaloneExternalAuthenticatorConfig _authConfig;

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
                CurrentTournament = dto.ToTournamentInfo(_config.PrizePool);
                return UniTask.FromResult(CurrentTournament);
            }
            return UniTask.FromResult<TournamentInfo?>(null);
        }
        public UniTask<TournamentFeeInfo?> GetRollingTournamentsFee(TournamentFeeRequestInfo[] requestData, CancellationToken ct = default) => UniTask.FromResult<TournamentFeeInfo?>(null);
        public UniTask<RollingTournamentHistory> GetRollingTournamentHistory(uint maxCount, uint skip = 0, CancellationToken ct = default) =>
            UniTask.FromResult(new RollingTournamentHistory(Array.Empty<RollingTournamentHistoryEntry>()));
        public UniTask<RollingTournamentSettlementStatus> GetTournamentSettlementStatus(CancellationToken ct = default) => UniTask.FromResult(new RollingTournamentSettlementStatus()
        {
            NewSettlements = 1,
        });
        public UniTask<TournamentInfo> SetActiveTournament(string tournamentId, CancellationToken ct = default) => UniTask.FromResult(new TournamentInfo());
        public UniTask<RollingTournamentDetails> GetRollingTournamentDetails(Guid matchId, CancellationToken ct = default) => UniTask.FromResult(new RollingTournamentDetails());

        public void OnWebMessage(WebMessage message)
        {
            if (!string.Equals(message.type, WebMessageTypes.TournamentUpdated))
                throw new Exception($"{nameof(WebGLTournamentCommunicator)} can handle only {WebMessageTypes.TournamentUpdated} event type.");

            var newTournamentData = JsonUtility.FromJson<TournamentUpdatedMessage>(message.message);
            CurrentTournament = newTournamentData.ToTournamentInfo();
            TournamentUpdated?.Invoke(CurrentTournament.Value);
        }
    }
}
