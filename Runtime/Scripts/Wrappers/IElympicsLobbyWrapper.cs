#nullable enable
using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.Models.Authentication;
using JetBrains.Annotations;

namespace ElympicsPlayPad.Wrappers
{
    internal interface IElympicsLobbyWrapper
    {
        public event Action<ElympicsState, ElympicsState>? ElympicsStateUpdated;
        IGameplaySceneMonitor GameplaySceneMonitor { get; }

        IRoomsManager RoomsManager { get; }

        AuthData? AuthData { get; }

        bool IsAuthenticated { get; }

        IWebSocketSession WebSocketSession { get; }

        void SignOut();

        UniTask ConnectStandaloneEditorToElympicsAsync(AuthData data, string region);
        UniTask ConnectToElympicsAsync(ConnectionData connectionData);
    }
}
