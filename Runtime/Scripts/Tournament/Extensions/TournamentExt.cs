#nullable enable
using System;
using System.Globalization;
using ElympicsPlayPad.Protocol.Responses;
using ElympicsPlayPad.Protocol.WebMessages;
using ElympicsPlayPad.Tournament.Data;

namespace ElympicsPlayPad.Tournament.Extensions
{
    public static class TournamentExt
    {
        public static TournamentInfo ToTournamentInfo(this TournamentResponse dto)
        {
            var startDate = DateTimeOffset.Parse(dto.startDate, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal);
            var endDate = DateTimeOffset.Parse(dto.endDate, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal);

            Guid? ownerId = string.IsNullOrEmpty(dto.ownerId) ? null : Guid.Parse(dto.ownerId);

            return new TournamentInfo
            {
                Id = dto.id,
                LeaderboardCapacity = dto.leaderboardCapacity,
                Name = dto.name,
                OwnerId = ownerId,
                StartDate = startDate,
                EndDate = endDate,
            };
        }

        public static TournamentInfo ToTournamentInfo(this TournamentUpdatedMessage dto)
        {
            var startDate = DateTimeOffset.Parse(dto.startDate, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal);
            var endDate = DateTimeOffset.Parse(dto.endDate, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal);

            Guid? ownerId = string.IsNullOrEmpty(dto.ownerId) ? null : Guid.Parse(dto.ownerId);

            return new TournamentInfo
            {
                Id = dto.id,
                LeaderboardCapacity = dto.leaderboardCapacity,
                Name = dto.name,
                OwnerId = ownerId,
                StartDate = startDate,
                EndDate = endDate,
            };
        }
    }

}
