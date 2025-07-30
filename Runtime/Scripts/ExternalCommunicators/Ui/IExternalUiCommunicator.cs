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
