#nullable enable

using ElympicsPlayPad.Lobby.InternalModels.FromLobby;
using ElympicsPlayPad.Lobby.InternalModels.ToLobby;

namespace ElympicsPlayPad.Lobby.Serializers
{
    internal interface ILobbySerializer
    {
        byte[] Serialize(IToLobby message);
        IFromLobby Deserialize(byte[] data);
    }
}
