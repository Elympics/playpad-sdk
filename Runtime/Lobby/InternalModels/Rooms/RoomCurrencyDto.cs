#nullable enable

using MessagePack;

namespace ElympicsPlayPad.Lobby.InternalModels.Rooms
{
    [MessagePackObject]
    public record RoomCurrencyDto(
        [property: Key(0)] string Ticker,
        [property: Key(1)] string? Address,
        [property: Key(2)] int Decimals,
        [property: Key(3)] string IconUrl);
}
