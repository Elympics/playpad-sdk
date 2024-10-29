using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.Models.Authentication;

namespace ElympicsPlayPad.Wrappers
{
    internal interface IElympicsLobbyWrapper
    {
        IGameplaySceneMonitor GameplaySceneMonitor { get; }

        IRoomsManager RoomsManager { get; }

        AuthData AuthData { get; }

        bool IsAuthenticated { get; }

        IWebSocketSession WebSocketSession { get; }

        void SignOut();

        UniTask ConnectToElympicsAsync(ConnectionData data);
    }
}
