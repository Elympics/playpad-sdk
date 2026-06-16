#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using ElympicsPlayPad.ExternalCommunicators.GameStatus.Models;

namespace ElympicsPlayPad.ExternalCommunicators.GameStatus
{
    public interface IExternalGameStatusCommunicator : IDisposable
    {
        event Action<PlayStatusInfo>? PlayStatusUpdated;
        PlayStatusInfo CurrentPlayStatus { get; }
        void HideSplashScreen();
        void ShowReconnectingScreen();
        void HideReconnectingScreen();
        [Obsolete("Replaced by new automatic RTT reporting system.", false)]
        void RttUpdated(TimeSpan rtt) { }
        UniTask<PlayStatusInfo> CanPlayGame(bool autoResolve, CancellationToken ct = default);
        /// <summary>Starts a quick match in the current tournament.</summary>
        /// <param name="config">Matchmaking configuration.</param>
        /// <returns>A reference to the newly created room once a match is found by the matchmaking system.</returns>
        /// <remarks>
        /// This method only works with tournaments. If you want to start a match outside of a tournament or
        /// use a different type of competitiveness, such as duels or rolling tournaments, consider using
        /// the <see cref="IRoomsManager.StartQuickMatch"/> method instead.
        /// </remarks>
        UniTask<IRoom> PlayGame(PlayGameConfig config, CancellationToken ct = default);
    }
}
