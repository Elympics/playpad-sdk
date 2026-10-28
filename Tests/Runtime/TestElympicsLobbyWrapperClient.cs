using System;
using System.Collections.Generic;
using System.Threading;
using Elympics;
using Elympics.Cysharp.Threading.Tasks;
using Elympics.Models.Authentication;
using Elympics.SnapshotAnalysis.Retrievers;
using ElympicsPlayPad.Auth.Models;
using ElympicsPlayPad.Lobby;
using ElympicsPlayPad.Rooms;
using ElympicsPlayPad.Wrappers;
using UnityEngine;

namespace ElympicsPlayPad.Tests
{
    internal class TestElympicsLobbyWrapperClient : MonoBehaviour, IElympicsLobbyWrapper
    {
        public event Action<ElympicsConnectionData> ElympicsConnectionEstablished;
        public event Action<ElympicsConnectionLostData> ElympicsConnectionLost;
        public ElympicsState State => IsAuthenticated ? ElympicsState.Connected : ElympicsState.Disconnected;
        public IRoomsManager RoomsManager { get; }
        public ElympicsUser? ElympicsUser => null;
        public string CurrentRegion { get; private set; } = string.Empty;
        public IReadOnlyCollection<string> AvailableRegions => null;
        public IReadOnlyCollection<CoinInfo> AvailableCoins => null;
        public bool ShouldLoadGameplaySceneAfterMatchmaking { get; set; }

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
        void IElympicsLobbyWrapper.WatchReplay(SnapshotAnalysisRetriever retriever) => throw new NotImplementedException();
        public UniTask ConnectToElympicsAsync(ConnectionData data) => throw new NotImplementedException();
        public UniTask<TournamentFeeInfo?> GetRollTournamentsFee(TournamentFeeRequestInfo[] requestData, CancellationToken ct = default) => throw new NotImplementedException();
    }

    public class MockWebSocket : IWebSocketSession
    {
        public bool IsConnected { get; private set; }

        public void ToggleConnection(bool connected) => IsConnected = connected;
    }
}
