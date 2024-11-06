using System;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.ExternalCommunicators.GameStatus.Models;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.GameStatus
{
    public abstract class CustomStandaloneGameStatusCommunicatorBase : MonoBehaviour, IExternalGameStatusCommunicator
    {
        public abstract PlayStatusInfo CurrentPlayStatus { get; }
        public event Action<PlayStatusInfo> PlayStatusUpdated;
        public abstract void Dispose();
        public abstract void HideSplashScreen();
        public abstract UniTask<PlayStatusInfo> CanPlayGame(bool autoResolve);
        public abstract void RttUpdated(TimeSpan rtt);
    }
}
