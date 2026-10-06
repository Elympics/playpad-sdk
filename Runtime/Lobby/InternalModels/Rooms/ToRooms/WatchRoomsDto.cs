#nullable enable

using System;
using ElympicsPlayPad.Lobby.InternalModels.ToLobby;
using MessagePack;

namespace ElympicsPlayPad.Lobby.InternalModels.Rooms.ToRooms
{
    [MessagePackObject]
    public record WatchRoomsDto() : LobbyOperation
    {
        [SerializationConstructor]
        public WatchRoomsDto(Guid operationId) : this() =>
            OperationId = operationId;
    }
}
