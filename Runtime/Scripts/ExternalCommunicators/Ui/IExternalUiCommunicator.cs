using Cysharp.Threading.Tasks;

namespace ElympicsPlayPad.ExternalCommunicators.Ui
{
    public interface IExternalUiCommunicator
    {
        UniTask Display(string name);

        #region helpers

        //TODO: Reevaluate after playpad determines availavle modals
        private const string TournamentRewards = "tournament/rewards";
        private const string TournamentTickets = "tournament/buy-tickets";
        private const string Trust = "trust";
        private const string TopBar = "island/topbar";

        UniTask DisplayTournamentRewards() => Display(TournamentRewards);
        UniTask DisplayTournamentTickets() => Display(TournamentTickets);
        UniTask DisplayTrust() => Display(Trust);
        UniTask DisplayTopBar() => Display(TopBar);

        #endregion
    }
}
