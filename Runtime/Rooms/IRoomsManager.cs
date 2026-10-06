#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.Lobby.InternalModels.FromLobby;
using ElympicsPlayPad.Rooms.Models;
using JetBrains.Annotations;

namespace ElympicsPlayPad.Rooms
{
    [PublicAPI]
    public interface IRoomsManager
    {
        /// <summary>Raised when the list of available rooms or the state of one of them changes.</summary>
        /// <remarks>Raised only between <see cref="StartTrackingAvailableRooms"/> and <see cref="StopTrackingAvailableRooms"/>.</remarks>
        /// <seealso cref="ListAvailableRooms"/>
        event Action<RoomListUpdatedArgs>? RoomListUpdated;
        event Action<JoinedRoomUpdatedArgs>? JoinedRoomUpdated;

        /// <summary>Raised when the local player leaves a room.</summary>
        event Action<LeftRoomArgs>? LeftRoom;

        /// <summary>Raised when another user joins <see cref="CurrentRoom"/>.</summary>
        event Action<UserJoinedArgs>? UserJoined;
        /// <summary>Raised when another user leaves <see cref="CurrentRoom"/>.</summary>
        event Action<UserLeftArgs>? UserLeft;
        event Action<UserCountChangedArgs>? UserCountChanged;
        event Action<HostChangedArgs>? HostChanged;
        event Action<UserReadinessChangedArgs>? UserReadinessChanged;
        event Action<UserChangedTeamArgs>? UserChangedTeam;
        event Action<CustomRoomDataChangedArgs>? CustomRoomDataChanged;
        event Action<CustomPlayerDataChangedArgs>? CustomPlayerDataChanged;
        event Action<RoomPublicnessChangedArgs>? RoomPublicnessChanged;
        event Action<RoomNameChangedArgs>? RoomNameChanged;
        event Action<RoomBetAmountChangedArgs>? RoomBetAmountChanged;
        event Action<MatchmakingDataChangedArgs>? MatchmakingDataChanged;

        event Action<MatchmakingStartedArgs>? MatchmakingStarted;
        event Action<MatchmakingEndedArgs>? MatchmakingEnded;
        event Action<MatchDataReceivedArgs>? MatchDataReceived;
        event Action<CustomMatchmakingDataChangedArgs>? CustomMatchmakingDataChanged;


        /// <seealso cref="ListAvailableRooms"/>
        bool TryGetAvailableRoom(Guid roomId, out IRoom? room);
        /// <remarks>
        /// All rooms that were created in the same region, on the same game version, are not <see cref="CurrentRoom"/>,
        /// and have <see cref="IRoom.IsQuickMatch"/> set to false are considered available.
        /// </remarks>
        IReadOnlyList<IRoom> ListAvailableRooms();

        /// <summary>Null if the local player is not in a room.</summary>
        IRoom? CurrentRoom { get; }

        /// <summary>Enables <see cref="RoomListUpdated"/>.</summary>
        UniTask StartTrackingAvailableRooms();
        UniTask StopTrackingAvailableRooms();
        bool IsTrackingAvailableRooms { get; }

        /// <summary>Creates a new room and joins it as the host.</summary>
        UniTask<IRoom> CreateAndJoinRoom(
            string roomName,
            string queueName,
            bool isSingleTeam,
            bool isPrivate,
            IReadOnlyDictionary<string, string>? customRoomData = null,
            IReadOnlyDictionary<string, string>? customMatchmakingData = null,
            IReadOnlyDictionary<string, string>? customPlayerData = null,
            CompetitivenessConfig? tournamentDetails = null);
        /// <param name="roomId">Optional when <paramref name="joinCode"/> is given.</param>
        /// <param name="joinCode">Required to join a private room.</param>
        /// <param name="teamIndex">Only applicable to rooms with more than one team.</param>
        UniTask<IRoom> JoinRoom(Guid? roomId, string? joinCode, uint? teamIndex = null, IReadOnlyDictionary<string, string>? customPlayerData = null);
        /// <summary>Creates and joins a room that starts matchmaking at once and is destroyed after the match ends.</summary>
        /// <returns>The room, once matchmaking finds a match.</returns>
        UniTask<IRoom> StartQuickMatch(
            string queueName,
            byte[]? gameEngineData = null,
            float[]? matchmakerData = null,
            Dictionary<string, string>? customRoomData = null,
            Dictionary<string, string>? customMatchmakingData = null,
            Dictionary<string, string>? customPlayerData = null,
            CompetitivenessConfig? competitivenessConfig = null,
            CancellationToken ct = default);
        event Func<IRoom, IRoom>? RoomSetUp;
        internal UniTask CheckJoinedRoomStatus(GameDataResponseDto gameDataResponse);
        internal void Reset();
    }
}
