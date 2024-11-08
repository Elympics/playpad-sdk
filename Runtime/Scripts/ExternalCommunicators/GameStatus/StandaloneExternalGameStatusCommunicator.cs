#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using ElympicsPlayPad.ExternalCommunicators.GameStatus.Models;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.GameStatus
{
    public class StandaloneExternalGameStatusCommunicator : IExternalGameStatusCommunicator
    {
        public event Action<PlayStatusInfo>? PlayStatusUpdated;
        public PlayStatusInfo CurrentPlayStatus { get; private set; }

        private readonly StandaloneExternalGameStatusConfig _config;
        private readonly IRoomsManager _roomsManager;

        public StandaloneExternalGameStatusCommunicator(StandaloneExternalGameStatusConfig config, IRoomsManager roomsManager)
        {
            _config = config;
            _roomsManager = roomsManager;
        }
        public UniTask<PlayStatusInfo> CanPlayGame(bool autoResolve, CancellationToken ct = default)
        {
            CurrentPlayStatus = new PlayStatusInfo()
            {
                PlayStatus = _config.PlayStatus,
                LabelInfo = _config.LabelMessage
            };
            return UniTask.FromResult(CurrentPlayStatus);
        }
        public async UniTask<IRoom> PlayGame(PlayGameConfig config, CancellationToken ct = default) => await _roomsManager.StartQuickMatch(config.QueueName, config.GameEngineData, config.MatchmakerData, config.CustomRoomData, config.CustomMatchmakingData, ct);
        public void RttUpdated(TimeSpan rtt)
        { }
        public void HideSplashScreen() => Debug.Log($"Hide splash screen.");
        public void Dispose()
        { }
    }
}
