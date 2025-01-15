using UnityEngine;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    public class EndGameView : MonoBehaviour
    {
        [SerializeField] private string lobbySceneName = "AsyncGameLobbyScene";

        public void Show()
        {
            gameObject.SetActive(true);
        }

        [UsedImplicitly] // by BackToLobbyButton
        public void ReturnToLobby()
        {
            SceneManager.LoadScene(lobbySceneName);
        }
    }
}
