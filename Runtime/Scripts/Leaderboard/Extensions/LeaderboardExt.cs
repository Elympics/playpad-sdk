#nullable enable
using System;
using System.Globalization;
using System.Linq;
using ElympicsPlayPad.Protocol.RequestResponse.Leaderboard;

namespace ElympicsPlayPad.Leaderboard.Extensions
{
    internal static class LeaderboardExt
    {
        public static LeaderboardStatus MapToLeaderboardStatus(this LeaderboardResponse response) => new()
        {

            Placements = response.entries?.Select(x => new Placement
            {
                UserId = x.userId,
                Nickname = x.nickname,
                Position = x.position,
                Points = x.points,
                ScoredAt = x.scoredAt,
                MatchId = x.matchId,
                TournamentId = string.IsNullOrEmpty(x.tournamentId) ? null : x.tournamentId
            }).ToArray(),
            TotalRecords = response.totalRecords,
            PageNumber = response.pageNumber
        };

        public static UserHighScore MapToUserHighScore(this UserHighScoreResponse response) => new()
        {
            Score = response.points,
            EndedAt = response.points == -1.0f ? null : DateTimeOffset.Parse(response.endedAt, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal)
        };
    }
}
