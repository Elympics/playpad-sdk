#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.Core.Logger;

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

    /// <summary>A match found by matchmaking (web lobby or Rooms), ready to be launched.</summary>
    internal sealed record FoundMatch(
        Guid MatchId,
        IReadOnlyList<Guid> MatchedPlayers,
        string TcpUdpServerAddress,
        string WebServerAddress,
        string UserSecret,
        IReadOnlyList<byte> GameEngineData,
        IReadOnlyList<float> MatchmakerData,
        string QueueName,
        string RegionName);

    internal static class MatchLaunching
    {
        /// <summary>A match without game server addresses is a single-player match.</summary>
        /// <param name="useWeb">Usually <see cref="ElympicsGameConfig.UseWeb"/>: picks the WebRTC or the TCP/UDP game server address.</param>
        public static async UniTask<ElympicsLaunchResult> LaunchAsync(IElympicsMatchLauncher launcher, string gameId, bool useWeb, FoundMatch match, Guid userId,
            CancellationToken ct = default)
        {
            // Recording the match below would overwrite the one the launcher is still running.
            if (launcher.State != MatchLauncherState.Idle)
                return ElympicsLaunchResult.Failed(string.Empty, ElympicsLaunchErrorCode.Busy, $"A match is already {launcher.State}.");

            ElympicsLogger.State.SetMatchId(match.MatchId.ToString());
            ElympicsLogger.State.SetQueue(match.QueueName);
            ElympicsLogger.State.SetGameServerAddress(match.TcpUdpServerAddress, match.WebServerAddress);
            LaunchedMatch.Set(match.MatchId, userId);

            ElympicsLaunchResult result;
            if (string.IsNullOrEmpty(match.WebServerAddress) && string.IsNullOrEmpty(match.TcpUdpServerAddress))
                result = await launcher.LaunchSinglePlayerAsync(gameId,
                    new SinglePlayerLaunchData(match.MatchId, userId, match.QueueName, match.RegionName, match.GameEngineData, match.MatchmakerData), ct: ct);
            else
            {
                var useTcpUdp = !useWeb;
                var data = new OnlineLaunchData(useTcpUdp ? match.TcpUdpServerAddress : match.WebServerAddress, useTcpUdp, match.MatchId, match.UserSecret, userId,
                    match.MatchedPlayers, match.RegionName, match.GameEngineData, match.MatchmakerData);
                result = await launcher.LaunchOnlineAsync(gameId, data, ct: ct);
            }

            if (!result.Success)
                LaunchedMatch.Clear();
            return result;
        }
    }
}
