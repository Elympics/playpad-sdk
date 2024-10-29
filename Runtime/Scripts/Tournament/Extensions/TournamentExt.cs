#nullable enable
using System;
using System.Globalization;
using System.Linq;
using ElympicsPlayPad.DTO;
using ElympicsPlayPad.Tournament.Data;
using UnityEngine;

namespace ElympicsPlayPad.Tournament.Extensions
{
    public static class TournamentExt
    {
        public static TournamentInfo? ToTournamentInfo(this TournamentDataDto dto)
        {
            if (string.IsNullOrEmpty(dto.id))
                return null;

            var startDate = DateTimeOffset.Parse(dto.startDate, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal);
            var endDate = DateTimeOffset.Parse(dto.endDate, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal);

            Guid? ownerId = string.IsNullOrEmpty(dto.ownerId) ? null : Guid.Parse(dto.ownerId);
            var state = (TournamentState)dto.state;

            var prizes = dto.prizes?.GroupBy(x => x.position).Select(group =>
            {
                var position = group.Key;
                var prize = group.ToArray().ToPrize(position);
                return prize;
            }).ToArray();

            return new TournamentInfo
            {
                Id = dto.id,
                LeaderboardCapacity = dto.leaderboardCapacity,
                Name = dto.name,
                LockedReason = dto.lockedReason,
                PrizePool = prizes,
                OwnerId = ownerId,
                State = state,
                StartDate = startDate,
                EndDate = endDate,
                TonDetails = dto.tonDetailDto?.ToTonDetails(),
                EvmDetails = dto.evmDetailsDto?.ToEvmDetails(),
            };
        }

        private static Prize ToPrize(this PrizeDto[] prizeArrayDto, int position)
        {
            if (prizeArrayDto.Length == 1)
                return prizeArrayDto[0].ToPrize();

            return new Prize
            {
                Name = null,
                Amount = null,
                Position = position,
                Texture = null,
                Prizes = prizeArrayDto.Select(x => x.ToPrize()).ToArray(),
            };
        }

        private static Prize ToPrize(this PrizeDto prizeDto)
        {
            Texture2D? rewardTexture = null;
            // ReSharper disable once InvertIf
            if (string.IsNullOrEmpty(prizeDto.image) is false)
            {
                try
                {
                    rewardTexture = new Texture2D(2, 2);
                    var image = Convert.FromBase64String(prizeDto.image);
                    var loadedImage = rewardTexture.LoadImage(image);
                    if (loadedImage is false)
                    {
                        Debug.LogWarning("Couldn't load png file.");
                        rewardTexture = null;
                    }
                }
                catch (Exception)
                {
                    Debug.LogError("Problem occured when loading image from bytes");
                    rewardTexture = null;
                }
            }

            return new Prize
            {
                Name = prizeDto.name,
                Amount = prizeDto.amount,
                Position = prizeDto.position,
                Texture = rewardTexture,
                Prizes = null,
            };
        }

        private static TonDetails? ToTonDetails(this TonDetailsDto tonDto)
        {
            if (string.IsNullOrEmpty(tonDto.tournamentAddress))
                return null;

            return new TonDetails
            {

                EntryFee = tonDto.entryFee,
                EntriesLeft = tonDto.entriesLeft,
                TournamentAddress = tonDto.tournamentAddress
            };
        }

        private static EvmDetails? ToEvmDetails(this EvmDetailsDto tonDto)
        {
            if (tonDto.dto == null
                || string.IsNullOrEmpty(tonDto.dto.chainId)
                || string.IsNullOrEmpty(tonDto.dto.address))
                return null;

            return new EvmDetails
            {

                RequiredNft = new Nft
                {
                    ChainId = tonDto.dto.chainId,
                    Address = tonDto.dto.address,
                }
            };
        }
    }
}
