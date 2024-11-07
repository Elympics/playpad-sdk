using UnityEngine;
using TMPro;
using Elympics;
using JetBrains.Annotations;
using Cysharp.Threading.Tasks;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    public class LobbyUIManager : MonoBehaviour
    {
        [SerializeField] private string playQueue = "training";

        [SerializeField] private TextMeshProUGUI playButtonText;

        [SerializeField] private GameObject authenticationInProgressScreen;
        [SerializeField] private GameObject matchmakingInProgressScreen;

        public void SetAuthenticationScreenActive(bool newValue) => authenticationInProgressScreen.SetActive(newValue);

        [UsedImplicitly]
        public void PlayGame()
        {
            matchmakingInProgressScreen.SetActive(true);
            ElympicsLobbyClient.Instance.RoomsManager.StartQuickMatch(playQueue).Forget();
        }
    }
}
