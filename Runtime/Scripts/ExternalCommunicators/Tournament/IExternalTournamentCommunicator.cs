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
        /// <summary>Sets <see cref="CurrentTournament"/> to the tournament with matching <paramref name="tournamentId"/>.</summary>
        /// <returns>Details of the new current tournament.</returns>
        /// <exception cref="ArgumentException">A tournament with the given <paramref name="tournamentId"/> does not exist.</exception>
        /// <remarks>
        /// This method does not raise <see cref="TournamentUpdated"/> event.
        /// PlayPad SDK does not provide a way to get an ID of a tournament that the local player has not joined.
        /// Joined tournaments include the daily tournament, tournaments created by the local player and the ones where the player has completed at least one match.
        /// To get an ID of any other tournament you can set up your own backend and fetch it from there.
        /// You can read more about integrating your own backend with Elympics here - https://docs.elympics.ai/deploy/advanced/external-game-backend/
        /// </remarks>
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
