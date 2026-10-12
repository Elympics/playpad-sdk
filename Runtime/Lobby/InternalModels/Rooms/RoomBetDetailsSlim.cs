#nullable enable

using System;
using MessagePack;

namespace ElympicsPlayPad.Lobby.InternalModels.Rooms
{
    [MessagePackObject]
    public record RoomBetDetailsSlim([property: Key(0)] string BetValue, [property: Key(1)] Guid CoinId, [property: Key(2)] Guid? RollingBetId);
}
