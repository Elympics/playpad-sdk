#nullable enable

using System;
using ElympicsPlayPad.Lobby.InternalModels.Rooms;
using ElympicsPlayPad.Rooms.Models;

namespace ElympicsPlayPad.Rooms
{
    internal static class RoomsMapper
    {
        public static ChainType Map(this ChainTypeDto dto) => dto switch
        {
            ChainTypeDto.TON => ChainType.TON,
            ChainTypeDto.EVM => ChainType.EVM,
            _ => throw new ArgumentOutOfRangeException(nameof(dto), dto, "Unknown enum value"),
        };

        public static LeavingReason Map(this LeavingReasonDto dto) => dto switch
        {
            LeavingReasonDto.RoomClosed => LeavingReason.RoomClosed,
            LeavingReasonDto.UserLeft => LeavingReason.UserLeft,
            _ => throw new ArgumentOutOfRangeException(nameof(dto), dto, "Unknown enum value"),
        };

        public static MatchmakingState Map(this MatchmakingStateDto dto) => dto switch
        {
            MatchmakingStateDto.Unlocked => MatchmakingState.Unlocked,
            MatchmakingStateDto.RequestingMatchmaking => MatchmakingState.RequestingMatchmaking,
            MatchmakingStateDto.Matchmaking => MatchmakingState.Matchmaking,
            MatchmakingStateDto.CancellingMatchmaking => MatchmakingState.CancellingMatchmaking,
            MatchmakingStateDto.Matched => MatchmakingState.Matched,
            MatchmakingStateDto.Playing => MatchmakingState.Playing,
            _ => throw new ArgumentOutOfRangeException(nameof(dto), dto, "Unknown enum value"),
        };

        public static MatchmakingStateDto Map(this MatchmakingState entity) => entity switch
        {
            MatchmakingState.Unlocked => MatchmakingStateDto.Unlocked,
            MatchmakingState.RequestingMatchmaking => MatchmakingStateDto.RequestingMatchmaking,
            MatchmakingState.Matchmaking => MatchmakingStateDto.Matchmaking,
            MatchmakingState.CancellingMatchmaking => MatchmakingStateDto.CancellingMatchmaking,
            MatchmakingState.Matched => MatchmakingStateDto.Matched,
            MatchmakingState.Playing => MatchmakingStateDto.Playing,
            _ => throw new ArgumentOutOfRangeException(nameof(entity), entity, "Unknown enum value"),
        };

        public static MatchData Map(this MatchDataDto dto) => new(
            dto.MatchId,
            dto.State.Map(),
            dto.MatchDetails?.Map(),
            dto.FailReason);

        public static MatchDataDto Map(this MatchData entity) => new(
            entity.MatchId,
            entity.State.Map(),
            entity.MatchDetails?.Map(),
            entity.FailReason);

        public static MatchDetails Map(this MatchDetailsDto dto) => new(
            dto.MatchedPlayersId,
            dto.TcpUdpServerAddress,
            dto.WebServerAddress,
            dto.UserSecret,
            dto.GameEngineData,
            dto.MatchmakerData);

        public static MatchDetailsDto Map(this MatchDetails entity) => new(
            entity.MatchedPlayersId,
            entity.TcpUdpServerAddress,
            entity.WebServerAddress,
            entity.UserSecret,
            entity.GameEngineData,
            entity.MatchmakerData);

        public static MatchState Map(this MatchStateDto dto) => dto switch
        {
            MatchStateDto.Initializing => MatchState.Initializing,
            MatchStateDto.Running => MatchState.Running,
            MatchStateDto.RunningEnded => MatchState.RunningEnded,
            MatchStateDto.InitializingFailed => MatchState.InitializingFailed,
            MatchStateDto.RunningFailed => MatchState.RunningFailed,
            MatchStateDto.ProcessCrashed => MatchState.ProcessCrashed,
            MatchStateDto.MatchmakingFailed => MatchState.MatchmakingFailed,
            _ => throw new ArgumentOutOfRangeException(nameof(dto), dto, "Unknown enum value"),
        };

        public static MatchStateDto Map(this MatchState entity) => entity switch
        {
            MatchState.Initializing => MatchStateDto.Initializing,
            MatchState.Running => MatchStateDto.Running,
            MatchState.RunningEnded => MatchStateDto.RunningEnded,
            MatchState.InitializingFailed => MatchStateDto.InitializingFailed,
            MatchState.RunningFailed => MatchStateDto.RunningFailed,
            MatchState.ProcessCrashed => MatchStateDto.ProcessCrashed,
            MatchState.MatchmakingFailed => MatchStateDto.MatchmakingFailed,
            _ => throw new ArgumentOutOfRangeException(nameof(entity), entity, "Unknown enum value"),
        };

        public static RoomBetDetails Map(this RoomBetDetailsDto dto) => new(
            dto.BetValueRaw,
            dto.Coin.Map(),
            dto.RollingBet?.Map());

        public static RoomCoin Map(this RoomCoinDto dto) => new()
        {
            CoinId = dto.CoinId,
            Chain = dto.Chain.Map(),
            Currency = dto.Currency.Map(),
        };

        public static RoomChain Map(this RoomChainDto dto) => new()
        {
            ExternalId = dto.ExternalId,
            Type = dto.Type.Map(),
            Name = dto.Name,
        };

        public static RoomCurrency Map(this RoomCurrencyDto dto) => new()
        {
            Ticker = dto.Ticker,
            Address = dto.Address,
            Decimals = dto.Decimals,
            IconUrl = dto.IconUrl,
        };

        public static RollingBet Map(this RollingBetDto dto) => new(
            dto.RollingBetId,
            dto.NumberOfPlayers,
            dto.EntryFee,
            dto.Prize);

        public static UserInfo Map(this UserInfoDto dto) => new(
            dto.TeamIndex,
            dto.IsReady,
            dto.CustomPlayerData,
            dto.User.ToPublicModel());
    }
}
