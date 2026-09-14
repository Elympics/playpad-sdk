#nullable enable
using ElympicsPlayPad.ExternalCommunicators.Authentication;
using ElympicsPlayPad.ExternalCommunicators.GameStatus;
using ElympicsPlayPad.ExternalCommunicators.Leaderboard;
using ElympicsPlayPad.ExternalCommunicators.Lobby;
using ElympicsPlayPad.ExternalCommunicators.Tournament;
using ElympicsPlayPad.ExternalCommunicators.Ui;
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
        public bool useCustomLobbyCommunicator = false;
        public CustomLobbyExternalCommunicator? customLobbyExternalCommunicator;
    }
}
