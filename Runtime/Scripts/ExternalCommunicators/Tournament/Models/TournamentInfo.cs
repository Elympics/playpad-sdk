#nullable enable
using System;
using UnityEngine;

namespace ElympicsPlayPad.Tournament.Data
{
    public readonly struct TournamentInfo
    {
        public string Id { get; init; }
        public int LeaderboardCapacity { get; init; }
        public string Name { get; init; }

        public PrizePoolInfo? PrizePool { get; init; }
        public Guid? OwnerId { get; init; }
        public DateTimeOffset StartDate { get; init; }
        public DateTimeOffset EndDate { get; init; }
        public override string ToString() => $"Id: {Id} Name: {Name} StartDate {StartDate:O} EndDate {EndDate:O}";
    }

    public readonly struct PrizePoolInfo
    {
        public string Type { get; init; }
        public string DisplayName { get; init; }
        public Texture2D? Image { get; init; }
        public float Amount { get; init; }
        public string Description { get; init; }
    }
}
