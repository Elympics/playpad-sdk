#nullable enable

using System.Threading;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.Rooms;
using UnityEngine;

namespace ElympicsPlayPad.Tests.Lobby.Common.RoomMocks
{
    internal class MatchLauncherMock : IMatchLauncher
    {
        public bool ShouldLoadGameplaySceneAfterMatchmaking { get; set; }

        public bool IsCurrentlyInMatch { get; set; }

        public void PlayMatch(RoomMatchData matchData) => PlayMatchCalledArgs = matchData;
        public UniTask StartMatchmaking(IRoom room)
        {
            Debug.Log($"[{nameof(MatchLauncherMock)}] Start matchmaking");
            return room.StartMatchmakingInternal();
        }
        public UniTask CancelMatchmaking(IRoom room, CancellationToken ct)
        {
            Debug.Log($"[{nameof(MatchLauncherMock)}] Matchmaking canceled");
            return room.CancelMatchmakingInternal(ct);
        }
        public void MatchmakingCompleted() => Debug.Log($"[{nameof(MatchLauncherMock)}] Match found");

        public RoomMatchData? PlayMatchCalledArgs;

        public void Reset()
        {
            ShouldLoadGameplaySceneAfterMatchmaking = false;
            IsCurrentlyInMatch = false;
            PlayMatchCalledArgs = null;
        }
    }
}
