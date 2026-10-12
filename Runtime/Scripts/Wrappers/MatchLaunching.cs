#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.Core.Logger;

namespace ElympicsPlayPad.Wrappers
{
    /// <summary>The identity of the launcher's current match, reported to the PlayPad host.</summary>
    internal static class LaunchedMatch
    {
        /// <summary>Empty when idle or when the match has no ID (e.g. half-remote).</summary>
        public static string MatchId(ElympicsMatchLauncher launcher) =>
            launcher.ActiveLaunchData?.EventMatchId is { } id && id != Guid.Empty ? id.ToString() : string.Empty;

        public static Guid? UserId(ElympicsMatchLauncher launcher) => launcher.ActiveLaunchData?.EventUserId;
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
        public static async UniTask<ElympicsLaunchResult> LaunchAsync(ElympicsMatchLauncher launcher, string gameId, bool useWeb, FoundMatch match, Guid userId,
            CancellationToken ct = default)
        {
            // Recording the match below would overwrite the logger state of the one the launcher is still running.
            if (launcher.State != MatchLauncherState.Idle)
                return ElympicsLaunchResult.Failed(string.Empty, ElympicsLaunchErrorCode.Busy, $"A match is already {launcher.State}.");

            ElympicsLogger.State.SetQueue(match.QueueName);
            ElympicsLogger.State.SetGameServerAddress(match.TcpUdpServerAddress, match.WebServerAddress);

            if (string.IsNullOrEmpty(match.WebServerAddress) && string.IsNullOrEmpty(match.TcpUdpServerAddress))
                return await launcher.LaunchSinglePlayerAsync(gameId,
                    new SinglePlayerLaunchData(match.MatchId, userId, match.QueueName, match.RegionName, match.GameEngineData, match.MatchmakerData), ct: ct);

            var useTcpUdp = !useWeb;
            var data = new OnlineLaunchData(useTcpUdp ? match.TcpUdpServerAddress : match.WebServerAddress, useTcpUdp, match.MatchId, match.UserSecret, userId,
                match.MatchedPlayers, match.RegionName, match.GameEngineData, match.MatchmakerData);
            return await launcher.LaunchOnlineAsync(gameId, data, ct: ct);
        }

        public static void ForgetLoggingFailure(this UniTask<ElympicsLaunchResult> launch, LoggerConfig logger) =>
            launch.ContinueWith(result =>
            {
                if (!result.Success)
                    logger.LogError(result.ToString());
            }).Forget(e => logger.LogException(e));
    }
}
