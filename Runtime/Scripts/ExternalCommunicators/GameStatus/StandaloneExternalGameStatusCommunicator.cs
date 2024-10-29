using System;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.ExternalCommunicators.GameStatus.Models;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.GameStatus
{
    public class StandaloneExternalGameStatusCommunicator : IExternalGameStatusCommunicator
    {
        public PlayStatusInfo CurrentPlayStatus => _currentPlayStatus;

        private PlayStatusInfo _currentPlayStatus;
        public event Action<PlayStatusInfo> PlayStatusUpdated;

        private readonly StandaloneExternalGameStatusConfig _config;

        public StandaloneExternalGameStatusCommunicator(StandaloneExternalGameStatusConfig config) => _config = config;
        public UniTask<PlayStatusInfo> CanPlayGame(bool autoResolve)
        {
            _currentPlayStatus = new PlayStatusInfo()
            {
                PlayStatus = _config.PlayStatus,
                LabelInfo = _config.LabelMessage
            };
            return UniTask.FromResult(_currentPlayStatus);
        }
        public void RttUpdated(TimeSpan rtt) => Debug.Log($"RttUpdated {rtt}");
        public void HideSplashScreen() => Debug.Log($"Application Initialized.");
        public void Dispose()
        { }
    }
}
