#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.Models.Authentication;
using UnityEngine;

namespace ElympicsPlayPad.Wrappers
{
    public class DefaultElympicsLobbyWrapper : MonoBehaviour, IElympicsLobbyWrapper
    {
        public event Action<ElympicsState, ElympicsState>? ElympicsStateUpdated;

        private void Awake()
        {
            ElympicsLobbyClient.Instance.StateChanged += OnStateChanged;
        }
        private void OnStateChanged(ElympicsState previousState, ElympicsState newState) => ElympicsStateUpdated?.Invoke(previousState, newState);

        public IGameplaySceneMonitor GameplaySceneMonitor => ElympicsLobbyClient.Instance!.GameplaySceneMonitor;
        public IRoomsManager RoomsManager => ElympicsLobbyClient.Instance!.RoomsManager;
        public AuthData? AuthData => ElympicsLobbyClient.Instance!.AuthData;
        public bool IsAuthenticated => ElympicsLobbyClient.Instance!.IsAuthenticated;
        public IWebSocketSession WebSocketSession => ElympicsLobbyClient.Instance!.WebSocketSession;
        public void SignOut() => ElympicsLobbyClient.Instance!.SignOut();
        public UniTask ConnectToElympicsAsync(ConnectionData data) => ElympicsLobbyClient.Instance!.ConnectToElympicsAsync(data);

        private void OnDestroy()
        {
            if (ElympicsLobbyClient.Instance != null)
                ElympicsLobbyClient.Instance.StateChanged -= OnStateChanged;
        }
    }
}
