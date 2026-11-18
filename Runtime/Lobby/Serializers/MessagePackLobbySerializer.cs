#nullable enable

using ElympicsPlayPad.Lobby.InternalModels.FromLobby;
using ElympicsPlayPad.Lobby.InternalModels.ToLobby;
using MessagePack;

namespace ElympicsPlayPad.Lobby.Serializers
{
    internal class MessagePackLobbySerializer : ILobbySerializer
    {
        private readonly MessagePackSerializerOptions _options;

        public byte[] Serialize(IToLobby message) =>
            MessagePackSerializer.Serialize(message, _options);

        public IFromLobby Deserialize(byte[] data) =>
            MessagePackSerializer.Deserialize<IFromLobby>(data, _options);

        public MessagePackLobbySerializer(MessagePackSerializerOptions options) =>
            _options = options;
    }
}
