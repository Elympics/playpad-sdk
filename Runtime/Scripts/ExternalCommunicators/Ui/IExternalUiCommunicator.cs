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
        UniTask DisplayTournamentsListing() => Display(TournamentsListing);

        #endregion
    }
}
