using ElympicsPlayPad.Lobby.InternalModels.FromLobby;
using ElympicsPlayPad.Lobby.InternalModels.ToLobby;
using MessagePack;

namespace ElympicsPlayPad.Lobby.InternalModels
{
    [MessagePackObject]
    public record PingDto : IFromLobby, IToLobby;
}
