#nullable enable
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.Models.Authentication;
using ElympicsPlayPad.Session;

namespace ElympicsPlayPad.Wrappers
{
    internal interface IElympicsLobbyWrapper : ISessionManagerAuthProvider
    {
        IGameplaySceneMonitor GameplaySceneMonitor { get; }

        IRoomsManager RoomsManager { get; }

        AuthData? AuthData { get; }

        IWebSocketSession WebSocketSession { get; }

        UniTask ConnectStandaloneEditorToElympicsAsync(AuthData data, string region);

        void WatchReplay();
    }
}
