#nullable enable

using System;
using MessagePack;

namespace ElympicsPlayPad.Lobby.InternalModels.ToLobby
{
    [MessagePackObject]
    public record ShowAuthDto() : LobbyOperation
    {
        [SerializationConstructor]
        public ShowAuthDto(Guid operationId) : this() => OperationId = operationId;
    }
}
