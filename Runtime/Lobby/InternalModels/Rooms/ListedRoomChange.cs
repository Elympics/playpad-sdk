#nullable enable

using System;
using MessagePack;

namespace ElympicsPlayPad.Lobby.InternalModels.Rooms
{
    [MessagePackObject]
    public record ListedRoomChange(
        [property: Key(0)] Guid RoomId,
        [property: Key(1)] PublicRoomState? PublicRoomState);

}
