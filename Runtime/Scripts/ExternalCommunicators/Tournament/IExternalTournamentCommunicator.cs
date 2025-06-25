#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using ElympicsPlayPad.Tournament.Data;

namespace ElympicsPlayPad.ExternalCommunicators.Tournament
{
    public interface IExternalTournamentCommunicator
    {
        TournamentInfo? CurrentTournament { get; }
        event Action<TournamentInfo>? TournamentUpdated;
        UniTask<TournamentInfo?> GetTournament(CancellationToken ct = default);
        UniTask<TournamentFeeInfo?> GetRollingTournamentsFee(TournamentFeeRequestInfo[] requestData, CancellationToken ct = default);

        /// <summary>
        /// Returns the history of rolling tournaments local user participated in.
        /// This includes currently active tournaments and the ones that have already ended.
        /// </summary>
        /// <param name="maxCount">Limits the number of fetched history entries.</param>
        /// <param name="skip">Number of most recent history entries to skip.</param>
        /// <param name="ct"></param>
        UniTask<RollingTournamentHistory> GetRollingTournamentHistory(uint maxCount, uint skip = 0, CancellationToken ct = default);
    }
}
