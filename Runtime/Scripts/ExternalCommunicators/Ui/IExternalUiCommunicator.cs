using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ElympicsPlayPad.ExternalCommunicators.Ui
{
    /// <summary>Allows opening PlayPad modal windows that are displayed over the game.</summary>
    /// <remarks>All methods of this interface return a <see cref="UniTask"/> which completes when the modal window opened by the method is closed.</remarks>
    public interface IExternalUiCommunicator
    {
        /// <inheritdoc cref="Display(string)"/>
        [Obsolete("Cancellation of the task returned by this method is no longer supported. Use the " + nameof(Display) + "(string) overload instead and implement cancellation separately if necessary.")]
        UniTask Display(string name, CancellationToken ct) => Display(name);

        /// <summary>Opens a PlayPad modal window displayed over the game.</summary>
        /// <param name="name">Name of the modal window to open.</param>
        /// <returns>An awaitable task which is completed when the modal window opened by this method is closed.</returns>
        /// <remarks>Most of the time using other methods from this interface is simpler than calling this method directly.</remarks>
        UniTask Display(string name);

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
