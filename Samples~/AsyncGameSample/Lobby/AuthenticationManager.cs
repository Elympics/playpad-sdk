using Cysharp.Threading.Tasks;
using UnityEngine;
using Elympics;
using System;
using ElympicsPlayPad.Session;
using ElympicsPlayPad.Session.Exceptions;
using ElympicsPlayPad.Web3;
using ElympicsPlayPad.Web3.Data;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    public class AuthenticationManager : MonoBehaviour
    {
        private SessionManager sessionManager;
        private Web3Wallet web3Wallet;
        private LobbyUIManager lobbyUIManager;

        public bool StartAuthenticationFinished { get; private set; } = false;

        public void InitializeAuthenticationManager(SessionManager sessionManager, Web3Wallet web3Wallet)
        {
            this.sessionManager = sessionManager;
            this.web3Wallet = web3Wallet;
        }

        public void SetLobbyUIManager(LobbyUIManager lobbyUIManager)
        {
            this.lobbyUIManager = lobbyUIManager;
        }

        public async UniTask AttemptStartAuthenticate()
        {
            ElympicsLobbyClient.Instance.WebSocketSession.Disconnected += (data) => OnConnectionChanged(data);
            ElympicsLobbyClient.Instance.WebSocketSession.Connected += () => OnConnectionChanged(null);

            lobbyUIManager.SetAuthenticationScreenActive(true);

            if (!ElympicsLobbyClient.Instance.IsAuthenticated || !ElympicsLobbyClient.Instance.WebSocketSession.IsConnected)
            {
                await sessionManager.AuthenticateFromExternalAndConnect();
            }

            lobbyUIManager.SetLobbyUIVariant(sessionManager);
            web3Wallet.WalletConnectionUpdated += ReactToAuthenticationChange;

            StartAuthenticationFinished = true;
        }

        public async UniTask AttemptReAuthenticate()
        {
            //await sessionManager.TryReAuthenticateIfWalletChanged();
            lobbyUIManager.SetLobbyUIVariant(sessionManager);
        }

        public async UniTask ConnectToWallet()
        {
            try
            {
                await sessionManager.ConnectToWallet();

                lobbyUIManager.SetLobbyUIVariant(sessionManager);
            }
            catch (WalletConnectionException)
            {
                sessionManager.ShowExternalWalletConnectionPanel();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void ReactToAuthenticationChange(WalletConnectionStatus status)
        {
            if (PersistentLobbyManager.Instance.IsAuthChangePossible)
            {
                AttemptReAuthenticate().Forget();
            }
        }

        private void OnConnectionChanged(DisconnectionData? disconnectionData)
        {
            if (!disconnectionData.HasValue)
            {
                lobbyUIManager.SetAuthenticationScreenActive(false);
            }
            else
            {
                if (disconnectionData.Value.Reason == DisconnectionReason.ClientRequest)
                    lobbyUIManager.SetAuthenticationScreenActive(true);
            }
        }
    }
}
