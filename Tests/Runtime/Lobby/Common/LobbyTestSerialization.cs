using ElympicsPlayPad.Wrappers;
using MessagePack;

namespace ElympicsPlayPad.Tests.Lobby.Common
{
    internal static class LobbyTestSerialization
    {
        public static readonly MessagePackSerializerOptions Options = DefaultElympicsLobbyWrapper.CreateSerializerOptions();
    }
}
