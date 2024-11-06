#nullable enable
namespace ElympicsPlayPad.Leaderboard
{
    public readonly struct LeaderboardStatusInfo
    {
        public Placement[]? Placements { get; init; }

        public Placement? UserPlacement { get; init; }
        public int Participants { get; init; }
    }

    public readonly struct Placement
    {
        public string UserId { get; init; }
        public string Nickname { get; init; }
        public int Position { get; init; }
        public float Score { get; init; }
        public string ScoredAt { get; init; }
        public string MatchId { get; init; }
        public string? TournamentId { get; init; }
    }
}
