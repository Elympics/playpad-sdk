#nullable enable

using System;
using System.Collections.Generic;
using ElympicsPlayPad.Lobby.InternalModels.ToLobby;
using MessagePack;

namespace ElympicsPlayPad.Lobby.InternalModels.Rooms.ToRooms
{
    [MessagePackObject]
    public record UpdateCustomPlayerDataDto(
        [property: Key(1)] Guid RoomId,
        [property: Key(2)] Dictionary<string, string> CustomPlayerData) : LobbyOperation
    {
        [SerializationConstructor]
        public UpdateCustomPlayerDataDto(Guid operationId, Guid roomId, Dictionary<string, string> customPlayerData) : this(roomId, customPlayerData) =>
            OperationId = operationId;
    }
}
