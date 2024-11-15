#nullable enable
using System;
using System.Globalization;
using ElympicsPlayPad.Protocol.Responses;
using ElympicsPlayPad.Protocol.WebMessages;
using ElympicsPlayPad.Tournament.Data;
using ElympicsPlayPad.Utility;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Tournament.Extensions
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
                PrizePool = dto.prizePool.ToPrizePoolInfo(),
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
                PrizePool = dto.prizePool.ToPrizePoolInfo(),
                Name = dto.name,
                OwnerId = ownerId,
                StartDate = startDate,
                EndDate = endDate,
            };
        }

        private static PrizePoolInfo? ToPrizePoolInfo(this PrizePoolResponse response)
        {

            if (string.IsNullOrEmpty(response.type))
                return null;

            var sprite = SpriteUtil.TryConvertToSprite(response.image);

            return new PrizePoolInfo
            {
                Amount = response.amount,
                DisplayName = response.displayName,
                Description = response.description,
                Type = response.type,
                Image = sprite
            };
        }

        private static PrizePoolInfo? ToPrizePoolInfo(this PrizePoolMessage response)
        {

            if (string.IsNullOrEmpty(response.type))
                return null;

            var sprite = SpriteUtil.TryConvertToSprite(response.image);

            return new PrizePoolInfo
            {
                Amount = response.amount,
                DisplayName = response.displayName,
                Description = response.description,
                Type = response.type,
                Image = sprite
            };
        }
    }
}
