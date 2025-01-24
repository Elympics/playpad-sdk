using UnityEngine;
using TMPro;
using JetBrains.Annotations;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    public class ErrorPopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI errorText;

        private bool forceRefresh;

        public void Show(string error, bool forceRefresh = false)
        {
            gameObject.SetActive(true);

            errorText.text = error;

            this.forceRefresh = forceRefresh;
        }

        [UsedImplicitly]
        public void Hide()
        {
            if (forceRefresh)
            {
#if UNITY_WEBGL
            Application.ExternalEval("document.location.reload(true)");
#endif
            }

            gameObject.SetActive(false);
        }
    }
}
