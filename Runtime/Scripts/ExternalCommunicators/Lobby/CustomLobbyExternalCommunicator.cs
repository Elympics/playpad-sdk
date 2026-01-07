using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.ExternalCommunicators.Lobby.Models;
using UnityEngine;
namespace ElympicsPlayPad.ExternalCommunicators.Lobby
{
    public abstract class CustomLobbyExternalCommunicator : MonoBehaviour, IExternalLobbyCommunicator
    {
        public abstract event Action<LobbyInfo> OnLobbyInfoUpdated;
        public abstract LobbyInfo Lobby { get; }
        public abstract UniTask<LobbyInfo> GetLobbyStatus(CancellationToken ct = default);
        public abstract void Quit(QuitArgs endGame);
        public abstract void PlayMatch();
    }
}
