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
        [Obsolete("Replaced by new automatic RTT reporting system.", false)]
        void RttUpdated(TimeSpan rtt) { }
        UniTask<PlayStatusInfo> CanPlayGame(bool autoResolve, CancellationToken ct = default);
        UniTask<IRoom> PlayGame(PlayGameConfig config, CancellationToken ct = default);
    }
}
