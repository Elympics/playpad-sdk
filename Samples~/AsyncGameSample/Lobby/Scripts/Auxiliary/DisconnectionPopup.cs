using Elympics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisconnectionPopup : MonoBehaviour
{
    [Header("Gameobject references")]
    [SerializeField] private Button okButton;
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private GameObject disconnectionPopupObject;

    [Header("Text to display")]
    [SerializeField] private string disconnectedMessage = "Disconnected from Main menu. Click ok to refresh.";

    private void Start()
    {
        if (ElympicsLobbyClient.Instance != null)
        {
            ElympicsLobbyClient.Instance.WebSocketSession.Disconnected -= WebSocketSession_Disconnected;
            ElympicsLobbyClient.Instance.WebSocketSession.Disconnected += WebSocketSession_Disconnected;
        }

        okButton.onClick.AddListener(RefreshWebsite);
    }

    private void OnDestroy()
    {
        if (ElympicsLobbyClient.Instance != null)
        {
            ElympicsLobbyClient.Instance.WebSocketSession.Disconnected -= WebSocketSession_Disconnected;
        }

        okButton.onClick.RemoveAllListeners();
    }

    private void WebSocketSession_Disconnected(DisconnectionData data)
    {
        Debug.LogError($"WEB SOCKET ERROR {data.Reason}");
        // Necesary check so that the popup doesn't show up when the user is connecting wallet via external applications or other intended web socket disconnects.
        if (data.Reason is DisconnectionReason.ClientRequest or DisconnectionReason.ApplicationShutdown)
            return;

        // Show this popup
        disconnectionPopupObject.SetActive(true);
        errorText.text = disconnectedMessage;
    }

    /// <summary>
    /// Method to force refresh website if in web gl or disable gameobject if in editor
    /// </summary>
    private void RefreshWebsite()
    {
#if UNITY_WEBGL
        Application.ExternalEval("document.location.reload(true)");
#else
        disconnectionPopupObject.SetActive(false);
#endif
    }
}
