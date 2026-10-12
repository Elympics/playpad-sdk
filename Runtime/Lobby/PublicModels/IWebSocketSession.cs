#nullable enable

namespace ElympicsPlayPad.Lobby
{
    public interface IWebSocketSession
    {
        bool IsConnected { get; }
    }
}
