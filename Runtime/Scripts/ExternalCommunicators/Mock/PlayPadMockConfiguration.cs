#nullable enable
using ElympicsPlayPad.ExternalCommunicators.Authentication;
using ElympicsPlayPad.ExternalCommunicators.GameStatus;
using ElympicsPlayPad.ExternalCommunicators.Lobby;
using ElympicsPlayPad.ExternalCommunicators.Ui;
using UnityEngine;

namespace ElympicsPlayPad
{
    [CreateAssetMenu(fileName = "PlayPadMockConfiguration", menuName = "Elympics/PlayPadMockConfiguration", order = 0)]
    public class PlayPadMockConfiguration : ScriptableObject
    {
        public bool useStandaloneAuthenticationCommunicator;
        public CustomStandaloneAuthenticationCommunicatorBase? customAuthenticatorCommunicator;
        public bool useCustomStandaloneGameStatusCommunicator;
        public CustomStandaloneGameStatusCommunicatorBase? customGameStatusCommunicator;
        public bool useCustomStandaloneExternalUiCommunicator;
        public CustomStandaloneExternalUiCommunicatorBase? customExternalUiCommunicator;
        public bool useCustomLobbyCommunicator;
        public CustomLobbyExternalCommunicator? customLobbyExternalCommunicator;
    }
}
