#nullable enable

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.Core.Logger;
using ElympicsPlayPad.Rooms;

namespace ElympicsPlayPad.Wrappers
{
    /// <summary>The match most recently launched by a PlayPad component. Empty when the launcher is idle or another caller launched the match.</summary>
    internal static class LaunchedMatch
    {
        public static string MatchId { get; private set; } = string.Empty;
        public static Guid? UserId { get; private set; }

        public static void Set(Guid matchId, Guid userId)
        {
            MatchId = matchId.ToString();
            UserId = userId;
        }

        public static void Clear()
        {
            MatchId = string.Empty;
            UserId = null;
        }
    }

    internal static class MatchLaunching
    {
        /// <summary>A match without game server addresses is a single-player match.</summary>
        public static async UniTask<ElympicsLaunchResult> LaunchAsync(IElympicsMatchLauncher launcher, ElympicsGameConfig gameConfig, RoomMatchData match, Guid userId,
            CancellationToken ct = default)
        {
            var details = match.MatchDetails;
            ElympicsLogger.State.SetMatchId(match.MatchId.ToString());
            ElympicsLogger.State.SetQueue(match.QueueName);
            ElympicsLogger.State.SetGameServerAddress(details.TcpUdpServerAddress, details.WebServerAddress);
            LaunchedMatch.Set(match.MatchId, userId);

            ElympicsLaunchResult result;
            if (string.IsNullOrEmpty(details.WebServerAddress) && string.IsNullOrEmpty(details.TcpUdpServerAddress))
                result = await launcher.LaunchSinglePlayerAsync(gameConfig.GameId,
                    new SinglePlayerLaunchData(match.MatchId, userId, match.QueueName, match.RegionName, details.GameEngineData, details.MatchmakerData), ct: ct);
            else
            {
                var useTcpUdp = !gameConfig.UseWeb;
                var data = new OnlineLaunchData(useTcpUdp ? details.TcpUdpServerAddress : details.WebServerAddress, useTcpUdp, match.MatchId, details.UserSecret, userId,
                    details.MatchedPlayersId, match.RegionName, details.GameEngineData, details.MatchmakerData);
                result = await launcher.LaunchOnlineAsync(gameConfig.GameId, data, ct: ct);
            }

            // A busy launcher keeps running the match it already has.
            if (!result.Success && result.Code != ElympicsLaunchErrorCode.Busy)
                LaunchedMatch.Clear();
            return result;
        }
    }
}
