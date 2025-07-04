#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
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
    }
}
