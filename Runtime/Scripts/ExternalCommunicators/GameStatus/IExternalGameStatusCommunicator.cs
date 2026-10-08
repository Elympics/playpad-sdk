#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
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
        UniTask<PlayStatusInfo> CanPlayGame(bool autoResolve, CancellationToken ct = default);
    }
}
