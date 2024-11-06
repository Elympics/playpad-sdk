#nullable enable
using System;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.ExternalCommunicators.GameStatus.Models;

namespace ElympicsPlayPad.ExternalCommunicators.GameStatus
{
    public interface IExternalGameStatusCommunicator : IDisposable
    {
        PlayStatusInfo CurrentPlayStatus { get; }
        public event Action<PlayStatusInfo>? PlayStatusUpdated;
        public void HideSplashScreen();
        public UniTask<PlayStatusInfo> CanPlayGame(bool autoResolve);
        public void RttUpdated(TimeSpan rtt);
    }
}
