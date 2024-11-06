#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using ElympicsPlayPad.ExternalCommunicators.GameStatus.Models;
using JetBrains.Annotations;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.GameStatus
{
    [PublicAPI]
    public abstract class CustomStandaloneGameStatusCommunicatorBase : MonoBehaviour, IExternalGameStatusCommunicator
    {
        public event Action<PlayStatusInfo>? PlayStatusUpdated;
        public abstract PlayStatusInfo CurrentPlayStatus { get; }
        public abstract void Dispose();
        public abstract void HideSplashScreen();
        public abstract UniTask<PlayStatusInfo> CanPlayGame(bool autoResolve);
        public abstract UniTask<IRoom> PlayGame(PlayGameConfig queueName, CancellationToken ct = default);
        public abstract void RttUpdated(TimeSpan rtt);
    }
}
