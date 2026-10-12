#nullable enable

using System;
using ElympicsPlayPad.Lobby.InternalModels.ToLobby;
using MessagePack;

namespace ElympicsPlayPad.Lobby.InternalModels.Rooms.ToRooms
{
    [MessagePackObject]
    public record UnwatchRoomsDto() : LobbyOperation
    {
        [SerializationConstructor]
        public UnwatchRoomsDto(Guid operationId) : this() =>
            OperationId = operationId;
    }
}
