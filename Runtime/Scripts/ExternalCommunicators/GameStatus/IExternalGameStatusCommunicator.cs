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
        public event Action<PlayStatusInfo>? PlayStatusUpdated;
        PlayStatusInfo CurrentPlayStatus { get; }
        public void HideSplashScreen();
        public UniTask<PlayStatusInfo> CanPlayGame(bool autoResolve);
        public UniTask<IRoom> PlayGame(PlayGameConfig config, CancellationToken ct = default);
        public void RttUpdated(TimeSpan rtt);
    }
}
