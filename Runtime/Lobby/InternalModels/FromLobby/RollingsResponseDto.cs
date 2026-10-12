#nullable enable

using System;
using System.Collections.Generic;
using MessagePack;

namespace ElympicsPlayPad.Lobby.InternalModels.FromLobby
{
    [MessagePackObject]
    public record RollingsResponseDto(
        [property: Key(0)] List<RollingResponseDto> Rollings,
        [property: Key(1)] Guid RequestId) : ILobbyResponse;
}
