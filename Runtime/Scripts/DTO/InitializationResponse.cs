#nullable enable
using System;

namespace ElympicsPlayPad.DTO
{
    [Serializable]
    internal class InitializationResponse
    {
        public AuthDataRaw authData;
        public string? error;
        public string device = null!;
        public string environment = null!;
        public int capabilities;
        public string closestRegion = null!;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public TournamentDataDto tournamentData;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }

    [Serializable]
    internal struct AuthDataRaw
    {
        public string jwt;
        public string userId;
        public string nickname;
    }

    [Serializable]
    public class TournamentDataDto
    {
        public string id = null!;
        public string gameId = null!;
        public string name = null!;
        public string ownerId = null!;
        public int state;
        public PrizeDto[]? prizes;
        public int leaderboardCapacity;
        public string createDate = null!;
        public string startDate = null!;
        public string endDate = null!;
        public bool isDefault;
        public string lockedReason = null!;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public TonDetailsDto tonDetailDto;
        public EvmDetailsDto evmDetailsDto;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }

    [Serializable]
    public class TonDetailsDto
    {
        public int entryFee;
        public int entriesLeft;
        public string tournamentAddress = null!;
    }

    [Serializable]
    public class EvmDetailsDto
    {
        public NftDto dto = null!;
    }

    [Serializable]
    public class PrizeDto
    {
        public string name = null!;
        public int position;
        public int amount;
        public string? image;
    }

    [Serializable]
    public class NftDto
    {
        public string chainId = null!;
        public string address = null!;
    }
}
