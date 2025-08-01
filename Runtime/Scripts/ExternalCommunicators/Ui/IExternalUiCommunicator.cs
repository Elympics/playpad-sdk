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
        private const string TonConnect = "ton/connect";
        private const string IslandExpand = "island/expand";
        private const string TonOnRamp = "ton/on-ramp";
        private const string EvmOnRamp = "evm/on-ramp";

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
        /// <summary>Displays a window asking the user to connect a TON wallet.</summary>
        UniTask DisplayTonConnect() => Display(TonConnect);
        /// <summary>Expands the PlayPad island.</summary>
        /// <remarks>Expanded view of the PlayPad island allows the user to connect or disconnect their TON wallet and to modify the balance of their battle wallet.</remarks>
        UniTask ExpandIsland() => Display(IslandExpand);
        /// <summary>Displays a window that allows the user to purchase coins on TON.</summary>
        UniTask DisplayTonOnRamp() => Display(TonOnRamp);
        /// <summary>Displays a window that allows the user to purchase coins on EVM.</summary>
        UniTask DisplayEvmOnRamp() => Display(EvmOnRamp);

        #endregion
    }
}
