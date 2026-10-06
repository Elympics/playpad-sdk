#nullable enable

using System;
using ElympicsPlayPad.Lobby.InternalModels.FromLobby;
using MessagePack;

namespace ElympicsPlayPad.Lobby.InternalModels.Rooms.FromRooms
{
    [MessagePackObject]
    public record RoomWasLeftDto(
        [property: Key(0)] Guid RoomId,
        [property: Key(1)] LeavingReasonDto Reason) : IFromLobby;
}
