#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.ExternalCommunicators.Lobby.Models;

namespace ElympicsPlayPad.ExternalCommunicators.Lobby
{
    public interface IExternalLobbyCommunicator
    {
        event Action<LobbyInfo>? OnLobbyInfoUpdated;
        LobbyInfo Lobby { get; }
        UniTask<LobbyInfo> GetLobbyStatus(CancellationToken ct = default);

        void Quit(QuitArgs endGame);
        void PlayMatch();
    }
}
