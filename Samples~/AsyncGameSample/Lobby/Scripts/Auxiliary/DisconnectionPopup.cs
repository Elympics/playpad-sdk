using ElympicsPlayPad.ExternalCommunicators;
using ElympicsPlayPad.Lobby;
using TMPro;
using UnityEngine;

public class DisconnectionPopup : MonoBehaviour
{
    [Header("Gameobject references")]
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private GameObject disconnectionPopupObject;

    [Header("Text to display")]
    [SerializeField] private string disconnectedMessage = "Disconnected. Check your internet connection and reload the page.";

    private void Start()
    {
        if (PlayPadCommunicator.Instance != null)
        {
            PlayPadCommunicator.Instance.Lobby.ElympicsConnectionLost -= OnConnectionLost;
            PlayPadCommunicator.Instance.Lobby.ElympicsConnectionLost += OnConnectionLost;
        }
    }

    private void OnDestroy()
    {
        if (PlayPadCommunicator.Instance != null)
            PlayPadCommunicator.Instance.Lobby.ElympicsConnectionLost -= OnConnectionLost;
    }

    private void OnConnectionLost(ElympicsConnectionLostData lostData)
    {
        var data = lostData.DisconnectionData;
        // Necesary check so that the popup doesn't show up when the user is connecting wallet via external applications or other intended web socket disconnects.
        if (data.Reason is DisconnectionReason.ClientRequest or DisconnectionReason.ApplicationShutdown)
        {
            Debug.Log($"Web socket disconnected: {data.Reason}");
            return;
        }

        Debug.LogError($"WEB SOCKET ERROR: {data.Reason}");

        // Show this popup
        disconnectionPopupObject.SetActive(true);
        errorText.text = disconnectedMessage;
    }
}
