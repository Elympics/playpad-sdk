#nullable enable

using System;
using System.Threading;
using Elympics.Cysharp.Threading.Tasks;
using ElympicsPlayPad.Lobby.InternalModels.FromLobby;
using ElympicsPlayPad.Lobby.InternalModels.ToLobby;

namespace ElympicsPlayPad.Lobby
{
    internal interface IWebSocketSessionInternal
    {
        event Action? Connected;
        event Action<IFromLobby>? MessageReceived;

        SessionConnectionDetails? ConnectionDetails { get; }
        UniTask<GameDataResponseDto> Connect(SessionConnectionDetails details, CancellationToken ct = default);
        void Disconnect(DisconnectionReason reason);
        UniTask<OperationResultDto> ExecuteOperation(LobbyOperation message, CancellationToken ct = default);
    }
}
