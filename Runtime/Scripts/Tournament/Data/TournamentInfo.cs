#nullable enable
using System;

namespace ElympicsPlayPad.Tournament.Data
{
    public readonly struct TournamentInfo
    {
        public string Id { get; init; }
        public int LeaderboardCapacity { get; init; }
        public string Name { get; init; }
        public string? LockedReason { get; init; }
        public Prize[]? PrizePool { get; init; }
        public Guid? OwnerId { get; init; }
        internal TournamentState State { get; init; }
        public DateTimeOffset StartDate { get; init; }
        public DateTimeOffset EndDate { get; init; }
        public TonDetails? TonDetails { get; init; }
        public EvmDetails? EvmDetails { get; init; }

        public override string ToString() => $"Id: {Id} Name: {Name} StartDate {StartDate:O} EndDate {EndDate:O}";
    }

    public struct TonDetails
    {
        public int EntryFee { get; init; }
        public int EntriesLeft { get; init; }
        public string TournamentAddress { get; init; }
    }

    public struct EvmDetails
    {
        public Nft RequiredNft { get; init; }
    }

    public struct Nft
    {
        public string ChainId { get; init; }
        public string Address { get; init; }
    }
}
