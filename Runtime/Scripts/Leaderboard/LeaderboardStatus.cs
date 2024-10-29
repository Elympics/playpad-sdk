#nullable enable
namespace ElympicsPlayPad.Leaderboard
{
    public readonly struct LeaderboardStatus
    {
        public Placement[]? Placements { get; init; }
        public int TotalRecords { get; init; }
        public int PageNumber { get; init; }
    }

    public readonly struct Placement
    {
        public string UserId { get; init; }
        public string Nickname { get; init; }
        public int Position { get; init; }
        public float Points { get; init; }
        public string ScoredAt { get; init; }
        public string MatchId { get; init; }
        public string? TournamentId { get; init; }
    }
}
