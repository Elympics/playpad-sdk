#nullable enable

using MessagePack;

namespace ElympicsPlayPad.Lobby.InternalModels.Rooms
{
    [MessagePackObject]
    public record RoomTournamentDetails([property: Key(0)] string TournamentId, [property: Key(1)] ChainTypeDto? ChainType);
}
