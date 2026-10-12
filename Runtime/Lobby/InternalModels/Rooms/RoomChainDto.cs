#nullable enable

using MessagePack;

namespace ElympicsPlayPad.Lobby.InternalModels.Rooms
{
    [MessagePackObject]
    public record RoomChainDto(
        [property: Key(0)] int ExternalId,
        [property: Key(1)] ChainTypeDto Type,
        [property: Key(2)] string Name);
}
