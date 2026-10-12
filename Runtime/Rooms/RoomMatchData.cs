#nullable enable

using System;
using ElympicsPlayPad.Rooms.Models;

namespace ElympicsPlayPad.Rooms
{
    internal sealed record RoomMatchData(Guid MatchId, MatchDetails MatchDetails, string QueueName, string RegionName);
}
