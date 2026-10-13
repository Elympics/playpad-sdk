#nullable enable

using System.Threading;
using Elympics.Cysharp.Threading.Tasks;

namespace ElympicsPlayPad.Rooms
{
    /// <summary>What Rooms needs from the lobby client that owns them.</summary>
    internal interface IMatchLauncher
    {
        bool ShouldLoadGameplaySceneAfterMatchmaking { get; }
        bool IsCurrentlyInMatch { get; }

        void PlayMatch(RoomMatchData matchData);

        UniTask StartMatchmaking(IRoom room);
        UniTask CancelMatchmaking(IRoom room, CancellationToken ct = default);
        void MatchmakingCompleted();
    }
}
