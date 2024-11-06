#nullable enable
using System;
using System.Globalization;
using System.Linq;
using ElympicsPlayPad.Protocol.Responses;
using ElympicsPlayPad.Protocol.WebMessages;

namespace ElympicsPlayPad.Leaderboard.Extensions
{
    internal static class LeaderboardExt
    {
        public static LeaderboardStatusInfo MapToLeaderboardStatus(this LeaderboardResponse response) => new()
        {
            Placements = response.entries?.Select(x => new Placement
            {
                UserId = x.userId,
                Nickname = x.nickname,
                Position = x.position,
                Score = x.score,
                ScoredAt = x.scoredAt,
                MatchId = x.matchId,
                TournamentId = string.IsNullOrEmpty(x.tournamentId) ? null : x.tournamentId
            }).ToArray(),
            UserPlacement = string.IsNullOrEmpty(response.userEntry.userId) ? null : new Placement
            {
                UserId = response.userEntry.userId,
                Nickname = response.userEntry.nickname,
                Position = response.userEntry.position,
                Score = response.userEntry.score,
                ScoredAt = response.userEntry.scoredAt,
                MatchId = response.userEntry.matchId,
                TournamentId = response.userEntry.tournamentId
            },
            Participants = response.participants,
        };

        public static LeaderboardStatusInfo MapToLeaderboardStatus(this LeaderboardUpdatedMessage response) => new()
        {
            Placements = response.entries?.Select(x => new Placement
            {
                UserId = x.userId,
                Nickname = x.nickname,
                Position = x.position,
                Score = x.score,
                ScoredAt = x.scoredAt,
                MatchId = x.matchId,
                TournamentId = string.IsNullOrEmpty(x.tournamentId) ? null : x.tournamentId
            }).ToArray(),
            UserPlacement = string.IsNullOrEmpty(response.userEntry.userId) ? null : new Placement
            {
                UserId = response.userEntry.userId,
                Nickname = response.userEntry.nickname,
                Position = response.userEntry.position,
                Score = response.userEntry.score,
                ScoredAt = response.userEntry.scoredAt,
                MatchId = response.userEntry.matchId,
                TournamentId = response.userEntry.tournamentId
            },
            Participants = response.participants,
        };

        public static UserHighScoreInfo MapToUserHighScore(this UserHighScoreResponse response) => new()
        {
            Points = response.points,
            ScoredAt = string.IsNullOrEmpty(response.endedAt) ? null : DateTime.Parse(response.endedAt),
        };

        public static UserHighScoreInfo MapToUserHighScore(this UserHighScoreUpdatedMessage response) => new()
        {
            Points = response.points,
            ScoredAt = string.IsNullOrEmpty(response.endedAt) ? null : DateTime.Parse(response.endedAt),
        };
    }
}
