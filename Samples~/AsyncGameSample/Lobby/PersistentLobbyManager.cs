using UnityEngine;
using Elympics;
using Cysharp.Threading.Tasks;
using System;
using ElympicsPlayPad.ExternalCommunicators;
using ElympicsPlayPad.Session;
using ElympicsPlayPad.Web3;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    public class PersistentLobbyManager : MonoBehaviour
    {
        private AuthenticationManager authenticationManager;
        private LobbyUIManager lobbyUIManager;

        private bool isAuthChangePossible = true;
        public bool IsAuthChangePossible => isAuthChangePossible;

        public Guid CachedMatchId { get; private set; }

        public static PersistentLobbyManager Instance;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        private void Start()
        {
            ElympicsLobbyClient.Instance!.RoomsManager.MatchDataReceived += RememberMatchId;

            authenticationManager = FindObjectOfType<AuthenticationManager>();
            GameObject elympicsExternalCommunicator = PlayPadCommunicator.Instance.gameObject;
            authenticationManager.InitializeAuthenticationManager(elympicsExternalCommunicator.GetComponent<SessionManager>(), elympicsExternalCommunicator.GetComponent<Web3Wallet>());
            SetLobbyUIManager();

            PlayPadCommunicator.Instance.GameStatusCommunicator?.ApplicationInitialized();
            authenticationManager.AttemptStartAuthenticate().Forget();
        }

        public void ChangeAuthAvialability(bool newState)
        {
            if (isAuthChangePossible == newState)
                return;

            isAuthChangePossible = newState;

            if (isAuthChangePossible && authenticationManager.StartAuthenticationFinished)
            {
                SetLobbyUIManager();
                authenticationManager.AttemptReAuthenticate().Forget();
            }
        }

        private void SetLobbyUIManager()
        {
            lobbyUIManager = FindObjectOfType<LobbyUIManager>();
            authenticationManager.SetLobbyUIManager(lobbyUIManager);
        }

        private void RememberMatchId(MatchDataReceivedArgs obj)
        {
            CachedMatchId = obj.MatchId;
        }

        public void ConnectToWallet() => authenticationManager.ConnectToWallet().Forget();
    }
}
