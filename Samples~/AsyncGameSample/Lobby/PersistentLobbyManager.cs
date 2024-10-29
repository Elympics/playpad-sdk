using UnityEngine;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.ExternalCommunicators;
using ElympicsPlayPad.Session;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    public class PersistentLobbyManager : MonoBehaviour
    {
        private AuthenticationManager authenticationManager;

        public static PersistentLobbyManager Instance;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            var communicator = PlayPadCommunicator.Instance;
            communicator.GameStatusCommunicator?.HideSplashScreen();

            authenticationManager = new AuthenticationManager();
            authenticationManager.InitialAuthentication(communicator.GetComponent<SessionManager>()).Forget();
        }
    }
}
