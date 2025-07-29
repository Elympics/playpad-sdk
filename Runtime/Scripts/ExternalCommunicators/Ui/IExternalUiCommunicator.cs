using System.Threading;
using Cysharp.Threading.Tasks;

namespace ElympicsPlayPad.ExternalCommunicators.Ui
{
    public interface IExternalUiCommunicator
    {
        UniTask Display(string name, CancellationToken ct = default);

        #region helpers

        private const string TournamentRewards = "tournament/rewards";
        private const string TournamentsListing = "tournaments/listing";

        UniTask DisplayTournamentRewards() => Display(TournamentRewards);
        /// <summary>
        /// Display a list of all active tournaments the local player has participated in.
        /// This list includes the daily tournament, tournaments created by the local player and ones the player has completed at least one match in.
        /// If the player selects one of the tournaments in the displayed list, it will be set as the current active tournament and
        /// <see cref="Tournament.IExternalTournamentCommunicator.TournamentUpdated"/> event will be raised.
        /// </summary>
        /// <remarks>
        /// If you want to set a tournament not contained in the list displayed by this method as the current tournament,
        /// you can use <see cref="Tournament.IExternalTournamentCommunicator.SetActiveTournament"/>.
        /// </remarks>
        UniTask DisplayTournamentsListing() => Display(TournamentsListing);

        #endregion
    }
}
