#nullable enable

using System;
using ElympicsPlayPad.Lobby.InternalModels;

namespace ElympicsPlayPad.Lobby
{
    internal static class LobbyMapper
    {
        public static ErrorKind Map(this ErrorKindDto dto) => dto switch
        {
            ErrorKindDto.Unspecified => ErrorKind.Unspecified,
            ErrorKindDto.GameDoesNotExist => ErrorKind.GameDoesNotExist,
            ErrorKindDto.GameVersionDoesNotExist => ErrorKind.GameVersionDoesNotExist,
            ErrorKindDto.QueueDoesNotExist => ErrorKind.QueueDoesNotExist,
            ErrorKindDto.RegionDoesNotExist => ErrorKind.RegionDoesNotExist,
            ErrorKindDto.RoomDoesNotExist => ErrorKind.RoomDoesNotExist,
            ErrorKindDto.RoomLocked => ErrorKind.RoomLocked,
            ErrorKindDto.RoomFull => ErrorKind.RoomFull,
            ErrorKindDto.TeamFull => ErrorKind.TeamFull,
            ErrorKindDto.Outdated => ErrorKind.Outdated,
            ErrorKindDto.AlreadyInRoom => ErrorKind.AlreadyInRoom,
            ErrorKindDto.NotInRoom => ErrorKind.NotInRoom,
            ErrorKindDto.RoomAlreadyExists => ErrorKind.RoomAlreadyExists,
            ErrorKindDto.RoomWithoutMatchmaking => ErrorKind.RoomWithoutMatchmaking,
            ErrorKindDto.AlreadyReady => ErrorKind.AlreadyReady,
            ErrorKindDto.AlreadyNotReady => ErrorKind.AlreadyNotReady,
            ErrorKindDto.NotHost => ErrorKind.NotHost,
            ErrorKindDto.NotEveryoneReady => ErrorKind.NotEveryoneReady,
            ErrorKindDto.AlreadyInTeam => ErrorKind.AlreadyInTeam,
            ErrorKindDto.InvalidTeam => ErrorKind.InvalidTeam,
            ErrorKindDto.FailedToUnlockAfterSuccessfulRemove => ErrorKind.FailedToUnlockAfterSuccessfulRemove,
            ErrorKindDto.FailedToCancelMatchmakingWithTimeout => ErrorKind.FailedToCancelMatchmakingWithTimeout,
            ErrorKindDto.RoomPrivate => ErrorKind.RoomPrivate,
            ErrorKindDto.OnPlayerReadyRejected => ErrorKind.OnPlayerReadyRejected,
            ErrorKindDto.OnStartMatchmakingRejected => ErrorKind.OnStartMatchmakingRejected,
            ErrorKindDto.MatchNotFound => ErrorKind.MatchNotFound,
            ErrorKindDto.CoinDoesNotExist => ErrorKind.CoinDoesNotExist,
            ErrorKindDto.ConfigurationNotValid => ErrorKind.ConfigurationNotValid,
            ErrorKindDto.AcceptedRoomIsOutdated => ErrorKind.AcceptedRoomIsOutdated,
            ErrorKindDto.RoomAlreadyInMatchedState => ErrorKind.RoomAlreadyInMatchedState,
            ErrorKindDto.JoinCodeMismatch => ErrorKind.JoinCodeMismatch,
            ErrorKindDto.Throttle => ErrorKind.Throttle,
            ErrorKindDto.InvalidMessage => ErrorKind.InvalidMessage,
            ErrorKindDto.ValidationError => ErrorKind.ValidationError,
            _ => throw new ArgumentOutOfRangeException(nameof(dto), dto, "Unknown enum value"),
        };

        public static ErrorKindDto Map(this ErrorKind entity) => entity switch
        {
            ErrorKind.Unspecified => ErrorKindDto.Unspecified,
            ErrorKind.GameDoesNotExist => ErrorKindDto.GameDoesNotExist,
            ErrorKind.GameVersionDoesNotExist => ErrorKindDto.GameVersionDoesNotExist,
            ErrorKind.QueueDoesNotExist => ErrorKindDto.QueueDoesNotExist,
            ErrorKind.RegionDoesNotExist => ErrorKindDto.RegionDoesNotExist,
            ErrorKind.RoomDoesNotExist => ErrorKindDto.RoomDoesNotExist,
            ErrorKind.RoomLocked => ErrorKindDto.RoomLocked,
            ErrorKind.RoomFull => ErrorKindDto.RoomFull,
            ErrorKind.TeamFull => ErrorKindDto.TeamFull,
            ErrorKind.Outdated => ErrorKindDto.Outdated,
            ErrorKind.AlreadyInRoom => ErrorKindDto.AlreadyInRoom,
            ErrorKind.NotInRoom => ErrorKindDto.NotInRoom,
            ErrorKind.RoomAlreadyExists => ErrorKindDto.RoomAlreadyExists,
            ErrorKind.RoomWithoutMatchmaking => ErrorKindDto.RoomWithoutMatchmaking,
            ErrorKind.AlreadyReady => ErrorKindDto.AlreadyReady,
            ErrorKind.AlreadyNotReady => ErrorKindDto.AlreadyNotReady,
            ErrorKind.NotHost => ErrorKindDto.NotHost,
            ErrorKind.NotEveryoneReady => ErrorKindDto.NotEveryoneReady,
            ErrorKind.AlreadyInTeam => ErrorKindDto.AlreadyInTeam,
            ErrorKind.InvalidTeam => ErrorKindDto.InvalidTeam,
            ErrorKind.FailedToUnlockAfterSuccessfulRemove => ErrorKindDto.FailedToUnlockAfterSuccessfulRemove,
            ErrorKind.FailedToCancelMatchmakingWithTimeout => ErrorKindDto.FailedToCancelMatchmakingWithTimeout,
            ErrorKind.RoomPrivate => ErrorKindDto.RoomPrivate,
            ErrorKind.OnPlayerReadyRejected => ErrorKindDto.OnPlayerReadyRejected,
            ErrorKind.OnStartMatchmakingRejected => ErrorKindDto.OnStartMatchmakingRejected,
            ErrorKind.MatchNotFound => ErrorKindDto.MatchNotFound,
            ErrorKind.CoinDoesNotExist => ErrorKindDto.CoinDoesNotExist,
            ErrorKind.ConfigurationNotValid => ErrorKindDto.ConfigurationNotValid,
            ErrorKind.AcceptedRoomIsOutdated => ErrorKindDto.AcceptedRoomIsOutdated,
            ErrorKind.RoomAlreadyInMatchedState => ErrorKindDto.RoomAlreadyInMatchedState,
            ErrorKind.JoinCodeMismatch => ErrorKindDto.JoinCodeMismatch,
            ErrorKind.Throttle => ErrorKindDto.Throttle,
            ErrorKind.InvalidMessage => ErrorKindDto.InvalidMessage,
            ErrorKind.ValidationError => ErrorKindDto.ValidationError,
            _ => throw new ArgumentOutOfRangeException(nameof(entity), entity, "Unknown enum value"),
        };

        public static ErrorBlame Map(this ErrorBlameDto dto) => dto switch
        {
            ErrorBlameDto.InternalServerError => ErrorBlame.InternalServerError,
            ErrorBlameDto.GameDeveloperError => ErrorBlame.GameDeveloperError,
            ErrorBlameDto.UserError => ErrorBlame.UserError,
            _ => throw new ArgumentOutOfRangeException(nameof(dto), dto, "Unknown enum value"),
        };

        public static ErrorBlameDto Map(this ErrorBlame entity) => entity switch
        {
            ErrorBlame.InternalServerError => ErrorBlameDto.InternalServerError,
            ErrorBlame.GameDeveloperError => ErrorBlameDto.GameDeveloperError,
            ErrorBlame.UserError => ErrorBlameDto.UserError,
            _ => throw new ArgumentOutOfRangeException(nameof(entity), entity, "Unknown enum value"),
        };
    }
}
