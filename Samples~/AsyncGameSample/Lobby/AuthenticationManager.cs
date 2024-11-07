using Cysharp.Threading.Tasks;
using UnityEngine;
using Elympics;
using ElympicsPlayPad.Session;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    public class AuthenticationManager
    {
        private LobbyUIManager cachedlobbyUIManager;
        private LobbyUIManager LobbyUIManager
        {
            get
            {
                if (cachedlobbyUIManager == null)
                    cachedlobbyUIManager = Object.FindObjectOfType<LobbyUIManager>(); // Must exist on the same scene
                return cachedlobbyUIManager;
            }
        }

        public async UniTask InitialAuthentication(SessionManager sessionManager)
        {
            sessionManager.StartSessionInfoUpdate += () => LobbyUIManager.SetAuthenticationScreenActive(true);
            sessionManager.FinishSessionInfoUpdate += () => LobbyUIManager.SetAuthenticationScreenActive(false);

            if (!ElympicsLobbyClient.Instance.IsAuthenticated || !ElympicsLobbyClient.Instance.WebSocketSession.IsConnected)
            {
                await sessionManager.AuthenticateFromExternalAndConnect();
            }
        }
    }
}
