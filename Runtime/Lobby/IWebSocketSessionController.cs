using Cysharp.Threading.Tasks;

namespace ElympicsPlayPad.Lobby
{
    internal interface IWebSocketSessionController
    {
        UniTask ReconnectIfPossible(DisconnectionData reason);
    }
}
