#nullable enable

using System;
using ElympicsPlayPad.Lobby.InternalModels.ToLobby;
using MessagePack;

namespace ElympicsPlayPad.Lobby.InternalModels.Rooms.ToRooms
{
    [MessagePackObject]
    public record LeaveRoomDto([property: Key(1)] Guid RoomId) : LobbyOperation
    {
        [SerializationConstructor]
        public LeaveRoomDto(Guid operationId, Guid roomId) : this(roomId) => OperationId = operationId;

        public virtual bool Equals(LeaveRoomDto? other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return RoomId == other.RoomId;
        }

        public override int GetHashCode() => HashCode.Combine(RoomId);
    }
}
