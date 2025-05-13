#nullable enable
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.Models.Authentication;
using UnityEngine;

namespace ElympicsPlayPad.Wrappers
{
    public class DefaultElympicsLobbyWrapper : MonoBehaviour, IElympicsLobbyWrapper
    {
        private IMatchLauncher _matchLauncher = null!;
        private void Awake()
        {
            if (ElympicsLobbyClient.Instance != null)
                _matchLauncher = ElympicsLobbyClient.Instance;
        }
        public IGameplaySceneMonitor GameplaySceneMonitor => ElympicsLobbyClient.Instance!.GameplaySceneMonitor;
        public IRoomsManager RoomsManager => ElympicsLobbyClient.Instance!.RoomsManager;
        public AuthData? AuthData => ElympicsLobbyClient.Instance!.AuthData;
        public bool IsAuthenticated => ElympicsLobbyClient.Instance!.IsAuthenticated;
        public IWebSocketSession WebSocketSession => ElympicsLobbyClient.Instance!.WebSocketSession;
        public void SignOut() => ElympicsLobbyClient.Instance!.SignOut();
        public void WatchReplay() => _matchLauncher.WatchReplay();
        public UniTask ConnectStandaloneEditorToElympicsAsync(AuthData data, string region)
        {
            var connectionData = new ConnectionData()
            {
                AuthType = AuthType.ClientSecret,
                Region = new RegionData(region)
            };
            return ElympicsLobbyClient.Instance!.ConnectToElympicsAsync(connectionData);
        }
        public UniTask ConnectToElympicsAsync(ConnectionData data) => ElympicsLobbyClient.Instance!.ConnectToElympicsAsync(data);
    }
}
