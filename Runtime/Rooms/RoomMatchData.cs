#nullable enable

using System;
using ElympicsPlayPad.Rooms.Models;

namespace ElympicsPlayPad.Rooms
{
    /// <summary>A match found by matchmaking in a room, ready to be played.</summary>
    internal sealed record RoomMatchData(Guid MatchId, MatchDetails MatchDetails, string QueueName, string RegionName);
}
