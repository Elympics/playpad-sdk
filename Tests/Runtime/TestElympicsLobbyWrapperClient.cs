using System;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.Models.Authentication;
using ElympicsPlayPad.Wrappers;
using UnityEngine;

namespace ElympicsPlayPad.Tests
{
    internal class TestElympicsLobbyWrapperClient : MonoBehaviour, IElympicsLobbyWrapper
    {
        public event Action<ElympicsState, ElympicsState> ElympicsStateUpdated;
        public IGameplaySceneMonitor GameplaySceneMonitor { get; } = new MockGameplaySceneMonitor();
        public IRoomsManager RoomsManager { get; }

        public AuthData AuthData { get; private set; }


        public bool IsAuthenticated => AuthData != null;

        public IWebSocketSession WebSocketSession
        {
            get
            {
                if (_mockWebSocket == null)
                {
                    _mockWebSocket = new MockWebSocket();
                    _mockWebSocket.ToggleConnection(false);
                }
                return _mockWebSocket;
            }
        }

        private MockWebSocket _mockWebSocket;

        public async UniTask Authenticate(AuthData cachedData, string region, bool autoRetry)
        {
            AuthData = cachedData;
            _mockWebSocket = new MockWebSocket();
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            _mockWebSocket.ToggleConnection(true);
        }
        public void SignOut()
        {
            AuthData = null;
            _mockWebSocket?.ToggleConnection(false);
        }
        public void WatchReplay() => throw new NotImplementedException();
        public UniTask ConnectStandaloneEditorToElympicsAsync(AuthData data, string region)
        {
            AuthData = data;
            _mockWebSocket = new MockWebSocket();
            _mockWebSocket.ToggleConnection(true);
            return UniTask.CompletedTask;
        }
    }

    public class MockWebSocket : IWebSocketSession
    {
        public event Action Connected;
        public event Action<DisconnectionData> Disconnected;
        public bool IsConnected { get; private set; }

        public void ToggleConnection(bool connected) => IsConnected = connected;
    }

    public class MockGameplaySceneMonitor : IGameplaySceneMonitor
    {
        public void Dispose()
        { }
        public bool IsCurrentlyInMatch { get; }
        public event Action GameplayStarted;
        public event Action GameplayFinished;
    }
}
