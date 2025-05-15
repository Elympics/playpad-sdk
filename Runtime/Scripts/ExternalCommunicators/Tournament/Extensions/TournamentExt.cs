#nullable enable
using System;
using System.Globalization;
using System.Linq;
using ElympicsPlayPad.ExternalCommunicators.VirtualDeposit.Models;
using ElympicsPlayPad.Protocol.Responses;
using ElympicsPlayPad.Protocol.WebMessages;
using ElympicsPlayPad.Tournament.Data;
using ElympicsPlayPad.Utility;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Tournament.Extensions
{
    internal static class TournamentExt
    {
        public static TournamentInfo ToTournamentInfo(this TournamentResponse dto, PrizePoolInfo? prizePoolInfo = null)
        {
            var startDate = DateTimeOffset.Parse(dto.startDate, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal);
            var endDate = DateTimeOffset.Parse(dto.endDate, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal);

            Guid? ownerId = string.IsNullOrEmpty(dto.ownerId) ? null : Guid.Parse(dto.ownerId);

            return new TournamentInfo
            {
                Id = dto.id,
                LeaderboardCapacity = dto.leaderboardCapacity,
                PrizePool = prizePoolInfo ?? dto.prizePool.ToPrizePoolInfo(),
                Name = dto.name,
                OwnerId = ownerId,
                StartDate = startDate,
                EndDate = endDate,
                IsDaily = dto.isDefault,
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
                IsDaily = dto.isDefault,
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

        public static FeeInfo ToTournamentFeeInfo(this TournamentFee feeResponse, CoinInfo coinInfo)
        {
            var decimals = coinInfo.Currency.Decimals;
            return new FeeInfo
            {
                EntryFeeRaw = string.IsNullOrEmpty(feeResponse.error) ? feeResponse.entryFee : null,
                EntryFee = string.IsNullOrEmpty(feeResponse.error) ? WeiConverter.FromWei(feeResponse.entryFee, decimals) : null,
                Error = feeResponse.error
            };
        }
    }
}
