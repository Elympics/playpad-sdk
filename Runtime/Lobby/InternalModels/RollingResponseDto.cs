#nullable enable

using System;
using MessagePack;

namespace ElympicsPlayPad.Lobby.InternalModels
{
    [MessagePackObject]
    public class RollingResponseDto
    {
        [Key(0)] public Guid RollingTournamentBetConfigId { get; set; }
        // Keys 1-3 and 6 (coin ID, player count, prize, prize distribution) are sent but not needed, so MessagePack skips them.
        [Key(4)] public string EntryFee { get; set; }
        [Key(5)] public string Error { get; set; }
    }
}
