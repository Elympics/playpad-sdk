#nullable enable

using System;
using System.Collections.Generic;
using ElympicsPlayPad.Rooms.Models;
using JetBrains.Annotations;

namespace ElympicsPlayPad.Rooms
{
    /// <param name="RoomIds">Rooms that were added, removed or had their state updated.</param>
    [PublicAPI] public record RoomListUpdatedArgs(IReadOnlyList<Guid> RoomIds);
    [PublicAPI] public record JoinedRoomUpdatedArgs(Guid RoomId);

    [PublicAPI] public record UserJoinedArgs(UserInfo User);
    [PublicAPI] public record UserLeftArgs(UserInfo User);
    [PublicAPI] public record UserCountChangedArgs(uint UserCount);
    [PublicAPI] public record HostChangedArgs(Guid UserId);
    [PublicAPI] public record UserReadinessChangedArgs(Guid UserId, bool IsReady);
    /// <param name="TeamIndex">Null only with experimental features.</param>
    [PublicAPI] public record UserChangedTeamArgs(Guid UserId, uint? TeamIndex);
    /// <param name="Value">Null if <paramref name="Key"/> was removed.</param>
    [PublicAPI] public record CustomRoomDataChangedArgs(string Key, string? Value);
    /// <param name="Value">Null if <paramref name="Key"/> was removed.</param>
    [PublicAPI] public record CustomPlayerDataChangedArgs(Guid UserId, string Key, string? Value);

    [PublicAPI] public record RoomPublicnessChangedArgs(bool IsPrivate);

    [PublicAPI] public record RoomBetAmountChangedArgs(RoomBetAmount? NewBetValue);

    [PublicAPI] public record RoomNameChangedArgs(string RoomName);

    [PublicAPI] public record LeftRoomArgs(Guid RoomId, LeavingReason Reason);

    [PublicAPI] public record MatchmakingStartedArgs();
    [PublicAPI] public record MatchmakingEndedArgs();
    [PublicAPI] public record MatchmakingDataChangedArgs();
    [PublicAPI] public record MatchDataReceivedArgs(Guid MatchId, string QueueName, MatchData MatchData);
    /// <param name="Value">Null if <paramref name="Key"/> was removed.</param>
    [PublicAPI] public record CustomMatchmakingDataChangedArgs(string Key, string? Value);
}
