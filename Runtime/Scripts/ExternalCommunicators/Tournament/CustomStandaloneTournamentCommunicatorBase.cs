#nullable enable
#pragma warning disable 67 //An event was declared but never used in the class in which it was declared.
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using ElympicsPlayPad.ExternalCommunicators.Tournament.Models.MyNamespace;
using ElympicsPlayPad.Tournament.Data;
using JetBrains.Annotations;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Tournament
{
    [PublicAPI]
    public abstract class CustomStandaloneTournamentCommunicatorBase : MonoBehaviour, IExternalTournamentCommunicator
    {
        public abstract TournamentInfo? CurrentTournament { get; }
        public event Action<TournamentInfo>? TournamentUpdated;
        public abstract UniTask<TournamentInfo?> GetTournament(CancellationToken ct = default);
        public abstract UniTask<TournamentFeeInfo?> GetRollingTournamentsFee(TournamentFeeRequestInfo[] requestData, CancellationToken ct = default);
        public abstract UniTask<RollingTournamentHistory> GetRollingTournamentHistory(uint maxCount, uint skip = 0, CancellationToken ct = default);
        public abstract UniTask<RollingTournamentSettlementStatus> GetTournamentSettlementStatus(CancellationToken ct = default);
        public abstract UniTask<TournamentInfo> SetActiveTournament(string tournamentId, CancellationToken ct = default);
        public abstract UniTask<RollingTournamentDetails> GetRollingTournamentDetails(Guid matchId, CancellationToken ct = default);
    }
}
