#nullable enable
using ElympicsPlayPad.ExternalCommunicators.Authentication;
using ElympicsPlayPad.ExternalCommunicators.GameStatus;
using ElympicsPlayPad.ExternalCommunicators.Leaderboard;
using ElympicsPlayPad.ExternalCommunicators.Lobby;
using ElympicsPlayPad.ExternalCommunicators.Tournament;
using ElympicsPlayPad.ExternalCommunicators.Ui;
using ElympicsPlayPad.ExternalCommunicators.VirtualDeposit;
using ElympicsPlayPad.ExternalCommunicators.Web3.Erc20SmartContract;
using ElympicsPlayPad.ExternalCommunicators.Web3.NFT;
using UnityEngine;

namespace ElympicsPlayPad
{
    [CreateAssetMenu(fileName = "PlayPadMockConfiguration", menuName = "Elympics/PlayPadMockConfiguration", order = 0)]
    public class PlayPadMockConfiguration : ScriptableObject
    {
        public bool useStandaloneAuthenticationCommunicator = false;
        public CustomStandaloneAuthenticationCommunicatorBase? customAuthenticatorCommunicator;
        public bool useCustomStandaloneLeaderboardCommunicator = false;
        public CustomStandaloneLeaderboardCommunicatorBase? customLeaderboardCommunicator;
        public bool useCustomStandaloneTournamentCommunicator = false;
        public CustomStandaloneTournamentCommunicatorBase? customTournamentCommunicator;
        public bool useCustomStandaloneGameStatusCommunicator = false;
        public CustomStandaloneGameStatusCommunicatorBase? customGameStatusCommunicator;
        public bool useCustomStandaloneExternalUiCommunicator = false;
        public CustomStandaloneExternalUiCommunicatorBase? customExternalUiCommunicator;
        public bool useCustomStandaloneErc20SmartContractCommunicator = false;
        public CustomStandaloneErc20SmartContractCommunicatorBase? customErc20SmartContractCommunicator;
        public bool useCustomStandaloneBlockChainCurrencyCommunicator = false;
        public CustomStandaloneBlockChainCurrencyCommunicatorBase? customBlockChainCurrencyCommunicator;
        public bool useCustomTonNftExternalCommunicator = false;
        public CustomTonNftExternalCommunicator? customTonNftExternalCommunicator;
        public bool useCustomEvmExternalCommunicator = false;
        public CustomEvmExternalCommunicator? customEvmExternalCommunicator;
        public bool useCustomLobbyCommunicator = false;
        public CustomLobbyExternalCommunicator? customLobbyExternalCommunicator;
    }
}
