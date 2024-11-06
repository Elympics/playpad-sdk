#nullable enable
using System;

namespace ElympicsPlayPad.Tournament.Data
{
    public readonly struct TournamentInfo
    {
        public string Id { get; init; }
        public int LeaderboardCapacity { get; init; }
        public string Name { get; init; }
        public Guid? OwnerId { get; init; }
        public DateTimeOffset StartDate { get; init; }
        public DateTimeOffset EndDate { get; init; }
        public override string ToString() => $"Id: {Id} Name: {Name} StartDate {StartDate:O} EndDate {EndDate:O}";
    }

}
