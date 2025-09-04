#nullable enable
using System;
using Elympics.Communication.Authentication.Models;

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
        public readonly int Position;
        public readonly float Score;
        public readonly string ScoredAt;
        public readonly string MatchId;
        public readonly string? TournamentId;
        public readonly ElympicsUser User;

        public Placement(int position, float score, string scoredAt, string matchId, string? tournamentId, ElympicsUser user)
        {
            Position = position;
            Score = score;
            ScoredAt = scoredAt;
            MatchId = matchId;
            TournamentId = tournamentId;
            User = user;
        }

        [Obsolete("Use" + nameof(User) + "." + nameof(ElympicsUser.UserId) + "instead.")]
        public string UserId => User.UserId.ToString();
        [Obsolete("Use" + nameof(User) + "." + nameof(ElympicsUser.Nickname) + "instead.")]
        public string Nickname => User.Nickname;
    }
}
