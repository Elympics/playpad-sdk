#nullable enable
using System;
using System.Globalization;
using System.Linq;
using Elympics.Communication.Authentication.Models;
using ElympicsPlayPad.Protocol.Responses;
using ElympicsPlayPad.Protocol.WebMessages;

namespace ElympicsPlayPad.Leaderboard.Extensions
{
    internal static class LeaderboardExt
    {
        public static LeaderboardStatusInfo MapToLeaderboardStatus(this LeaderboardResponse response)
        {
            var userEntry = response.userEntry;
            return new LeaderboardStatusInfo
            {
                Placements = response.entries?.Select(x => new Placement(
                    x.position,
                    x.score,
                    x.scoredAt,
                    x.matchId,
                    string.IsNullOrEmpty(x.tournamentId) ? null : x.tournamentId,
                    new ElympicsUser(Guid.Parse(x.userId), x.nickname, NicknameStatus.Unknown, "todo: add avatar URL")
                )).ToArray(),
                UserPlacement = string.IsNullOrEmpty(userEntry.userId) ? null : new Placement(
                    userEntry.position,
                    userEntry.score,
                    userEntry.scoredAt,
                    userEntry.matchId,
                    userEntry.tournamentId,
                    new ElympicsUser(Guid.Parse(userEntry.userId), userEntry.nickname, NicknameStatus.Unknown, "todo: add avatar URL")
                ),
                Participants = response.participants,
            };
        }

        public static LeaderboardStatusInfo MapToLeaderboardStatus(this LeaderboardUpdatedMessage response) => new()
        {
            Placements = response.entries?.Select(x => new Placement
            (
                x.position,
                x.score,
                x.scoredAt,
                x.matchId,
                string.IsNullOrEmpty(x.tournamentId) ? null : x.tournamentId,
                new ElympicsUser(Guid.Parse(x.userId), x.nickname, NicknameStatus.Unknown, "todo: add avatar URL")
            )).ToArray(),
            UserPlacement = string.IsNullOrEmpty(response.userEntry.userId) ? null : new Placement
            (
                response.userEntry.position,
                response.userEntry.score,
                response.userEntry.scoredAt,
                response.userEntry.matchId,
                response.userEntry.tournamentId,
                new ElympicsUser(Guid.Parse(response.userEntry.userId),response.userEntry.nickname, NicknameStatus.Unknown, "todo: add avatar URL")
            ),
            Participants = response.participants,
        };

        public static UserHighScoreInfo? MapToUserHighScore(this UserHighScoreResponse response)
        {
            if (string.IsNullOrEmpty(response.points))
                return null;

            return new UserHighScoreInfo
            {

                Points = float.Parse(response.points, CultureInfo.InvariantCulture),
                ScoredAt = string.IsNullOrEmpty(response.endedAt) ? null : DateTime.Parse(response.endedAt),
            };
        }

        public static UserHighScoreInfo? MapToUserHighScore(this UserHighScoreUpdatedMessage response)
        {
            if (string.IsNullOrEmpty(response.points))
                return null;

            return new UserHighScoreInfo
            {

                Points = float.Parse(response.points, CultureInfo.InvariantCulture),
                ScoredAt = string.IsNullOrEmpty(response.endedAt) ? null : DateTime.Parse(response.endedAt),
            };
        }
    }
}
