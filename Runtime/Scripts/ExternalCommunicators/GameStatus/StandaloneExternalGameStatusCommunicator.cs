#nullable enable
#pragma warning disable 67 //An event was declared but never used in the class in which it was declared.
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.Rooms.Models;
using ElympicsPlayPad.ExternalCommunicators.GameStatus.Exceptions;
using ElympicsPlayPad.ExternalCommunicators.GameStatus.Models;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.GameStatus
{
    public class StandaloneExternalGameStatusCommunicator : CustomStandaloneGameStatusCommunicatorBase, IDisposable
    {
        public event Action<PlayStatusInfo>? PlayStatusUpdated;
        public override PlayStatusInfo CurrentPlayStatus => _currentPlayStatus;

        private PlayStatusInfo _currentPlayStatus;

        [SerializeField] private StandaloneExternalGameStatusConfig _config;
        private IRoomsManager _roomsManager;
        private Dictionary<string, string> _finalCustomMatchmakingData = new();

        private void Awake() => _roomsManager = ElympicsLobbyClient.Instance.RoomsManager;

        public override UniTask<PlayStatusInfo> CanPlayGame(bool autoResolve, CancellationToken ct = default)
        {
            _currentPlayStatus = new PlayStatusInfo()
            {
                PlayStatus = _config.PlayStatus,
                LabelInfo = _config.LabelMessage,
                IsHintAvailable = _config.IsHingAvailable,
            };
            return UniTask.FromResult(CurrentPlayStatus);
        }
        public override async UniTask<IRoom> PlayGame(PlayGameConfig config, CancellationToken ct = default)
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

        public override void HideSplashScreen() => Debug.Log($"Hide splash screen.");
        public override void Dispose()
        { }
    }
}
