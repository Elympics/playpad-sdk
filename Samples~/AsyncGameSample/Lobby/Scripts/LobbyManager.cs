using Cysharp.Threading.Tasks;
using ElympicsPlayPad.ExternalCommunicators;
using ElympicsPlayPad.Rooms.Models;
using ElympicsPlayPad.Session;
using UnityEngine;
using UnityEngine.Assertions;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    public class LobbyManager : MonoBehaviour
    {
        [SerializeField] private TournamentView tournamentView;
        [SerializeField] private LeaderboardView leaderboardView;
        [SerializeField] private AllTimeHighScoreView allTimeHighScoreView;
        [SerializeField] private TournamentPlayButton tournamentPlayButton;

        [SerializeField] private GameObject authenticationInProgressScreen;

        private SessionManager _sessionManager;

        private void Start()
        {
            _sessionManager = FindObjectOfType<SessionManager>();
            Assert.IsNotNull(_sessionManager);

            OnLobbySceneLoaded().Forget();
        }

        private async UniTask OnLobbySceneLoaded()
        {
            _sessionManager.StartSessionInfoUpdate += SetAuthenticationScreenActive;
            _sessionManager.FinishSessionInfoUpdate += SetAuthenticationScreenInactive;
            _sessionManager.FinishSessionInfoUpdate += RejoinIfHasOngoingMatch;

            var shouldHideSplashScreen = false;

            if (!_sessionManager.ConnectedWithPlayPad)
            {
                await _sessionManager.AuthenticateFromExternalAndConnect();
                shouldHideSplashScreen = true;
            }

            // Put here any further lobby scene configuration and UI adjustments
            tournamentView.OnStart();
            leaderboardView.OnStart();
            allTimeHighScoreView.OnStart();
            tournamentPlayButton.OnStart();
            ElympicsBestScoreManager.OnStart();

            if (shouldHideSplashScreen)
                PlayPadCommunicator.Instance.GameStatusCommunicator?.HideSplashScreen();
        }

        private void SetAuthenticationScreenActive() => authenticationInProgressScreen.SetActive(true);
        private void SetAuthenticationScreenInactive() => authenticationInProgressScreen.SetActive(false);

        // If the game dependens on local state too much, making the game rejoinable won't be possible
        // It should be managed in the GenericSoloServerHandler whether the server closes as soon as the player disconnects (allowing for rejoin) or not
        private void RejoinIfHasOngoingMatch()
        {
            var currentRoom = PlayPadCommunicator.Instance?.Lobby.RoomsManager.CurrentRoom;

            if (currentRoom?.State.MatchmakingData?.MatchData?.State == MatchState.Running)
                currentRoom.PlayAvailableMatch();
        }

        private void OnDestroy()
        {
            _sessionManager.StartSessionInfoUpdate -= SetAuthenticationScreenActive;
            _sessionManager.FinishSessionInfoUpdate -= SetAuthenticationScreenInactive;
            _sessionManager.FinishSessionInfoUpdate -= RejoinIfHasOngoingMatch;
        }
    }
}
