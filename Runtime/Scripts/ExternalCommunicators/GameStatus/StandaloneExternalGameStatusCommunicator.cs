#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.Rooms.Models;
using ElympicsPlayPad.ExternalCommunicators.GameStatus.Exceptions;
using ElympicsPlayPad.ExternalCommunicators.GameStatus.Models;
using ElympicsPlayPad.ExternalCommunicators.Tournament.Utility;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.GameStatus
{
    public class StandaloneExternalGameStatusCommunicator : IExternalGameStatusCommunicator
    {
        public event Action<PlayStatusInfo>? PlayStatusUpdated;
        public PlayStatusInfo CurrentPlayStatus { get; private set; }

        private readonly StandaloneExternalGameStatusConfig _config;
        private readonly IRoomsManager _roomsManager;
        private Dictionary<string, string> _finalCustomMatchmakingData = new();

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
                LabelInfo = _config.LabelMessage,
                IsHintAvailable = _config.IsHingAvailable
            };
            return UniTask.FromResult(CurrentPlayStatus);
        }
        public async UniTask<IRoom> PlayGame(PlayGameConfig config, CancellationToken ct = default)
        {
            if (_config.PlayStatus != 0)
                throw new GameStatusException($"Can't start game. ErrorCode: {_config.PlayStatus} Reason: {_config.LabelMessage}");

            _finalCustomMatchmakingData.Clear();
            // ReSharper disable once InvertIf
            if (config.CustomMatchmakingData != null)
            {
                _finalCustomMatchmakingData.AddRange(config.CustomMatchmakingData);
                _ = _finalCustomMatchmakingData.Remove(TournamentConst.TournamentIdKey);
            }

            return await _roomsManager.StartQuickMatch(config.QueueName, config.GameEngineData, config.MatchmakerData, config.CustomRoomData, _finalCustomMatchmakingData, ct: ct);
        }

        public void HideSplashScreen() => Debug.Log($"Hide splash screen.");
        public void Dispose()
        { }
    }
}
