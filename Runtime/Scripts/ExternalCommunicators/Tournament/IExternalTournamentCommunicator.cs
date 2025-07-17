#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using ElympicsPlayPad.ExternalCommunicators.Tournament.Models.MyNamespace;
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
        UniTask<RollingTournamentHistory> GetRollingTournamentHistory(uint maxCount, uint skip = 0, CancellationToken ct = default);
        UniTask<RollingTournamentSettlementStatus> GetTournamentSettlementStatus(CancellationToken ct = default);
        UniTask<TournamentInfo> SetActiveTournament(string tournamentId, CancellationToken ct = default);
        /// <summary>Returns detailed information about a rolling tournament in which the local player participated.</summary>
        /// <param name="matchId">ID of a match that was played by the local player as part of a rolling tournament.</param>
        /// <remarks>
        /// This method takes an ID of a match as a parameter, but returns detailed information about the current state of an entire rolling tournament instance.
        /// A typical use for this method is to fetch that information after a player finishes a match as part of a rolling tournament, so it can be displayed at a match summary screen.
        /// When using this method in that way, keep in mind that <see cref="RollingTournamentDetails.State"/> may have a value of <see cref="RollingTournamentDetails.TournamentState.YourResultsPending"/>.
        /// </remarks>
        UniTask<RollingTournamentDetails> GetRollingTournamentDetails(string matchId, CancellationToken ct = default);
    }
}
