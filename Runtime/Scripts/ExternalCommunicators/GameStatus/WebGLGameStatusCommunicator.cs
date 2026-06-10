#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.AssemblyCommunicator;
using Elympics.AssemblyCommunicator.Events;
using Elympics.Communication.Rooms.PublicModels;
using Elympics.Core.Logger;
using Elympics.Rooms.Models;
using ElympicsPlayPad.ExternalCommunicators.GameStatus.Exceptions;
using ElympicsPlayPad.ExternalCommunicators.GameStatus.Models;
using ElympicsPlayPad.ExternalCommunicators.Tournament;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Requests;
using ElympicsPlayPad.Protocol.Responses;
using ElympicsPlayPad.Protocol.VoidMessages;
using ElympicsPlayPad.Protocol.WebMessages;
using ElympicsPlayPad.Utility;
using ElympicsPlayPad.Wrappers;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.GameStatus
{
    internal class WebGLGameStatusCommunicator : IExternalGameStatusCommunicator, IWebMessageReceiver, IElympicsObserver<ElympicsStateChanged>
    {
        public PlayStatusInfo CurrentPlayStatus { get; private set; }
        public event Action<PlayStatusInfo>? PlayStatusUpdated;
        private readonly PlayPadMessagingSystem _playPadMessagingSystem;
        private readonly IElympicsLobbyWrapper _lobby;
        private readonly IExternalTournamentCommunicator _tournamentCommunicator;
        private readonly IRoomsManager _roomsManager;
        private readonly Dictionary<string, string> _joinedCustomMatchmakingData = new();
        private readonly LoggerConfig _logger = ElympicsLogger.WithPlayPadSdkService().WithClass(typeof(WebGLGameStatusCommunicator));

        public WebGLGameStatusCommunicator(
            PlayPadMessagingSystem playPadMessagingSystem,
            IElympicsLobbyWrapper lobby,
            IExternalTournamentCommunicator tournamentCommunicator)
        {
            _playPadMessagingSystem = playPadMessagingSystem;
            _playPadMessagingSystem.RegisterIWebEventReceiver(this, WebMessageTypes.PlayStatusUpdated);
            _lobby = lobby;
            _tournamentCommunicator = tournamentCommunicator;
            _lobby.GameplaySceneMonitor.GameplayStarted += SendSystemInfoData;
            CrossAssemblyEventBroadcaster.AddObserver(this);
            _roomsManager = _lobby.RoomsManager;
        }

        public void HideSplashScreen() => _playPadMessagingSystem.SendVoidMessage<EmptyPayload>(VoidMessageTypes.HideSplashScreen);

        public void ShowReconnectingScreen() => _playPadMessagingSystem.SendVoidMessage<EmptyPayload>(VoidMessageTypes.ShowReconnectingScreen);
        public void HideReconnectingScreen() => _playPadMessagingSystem.SendVoidMessage<EmptyPayload>(VoidMessageTypes.HideReconnectingScreen);

        public async UniTask<PlayStatusInfo> CanPlayGame(bool autoResolve, CancellationToken ct = default)
        {
            var request = new CanPlayGameRequest
            {
                autoResolve = autoResolve,
            };
            var response = await _playPadMessagingSystem.SendRequestMessage<CanPlayGameRequest, CanPlayGameResponse>(RequestResponseMessageTypes.GetPlayStatus, request, ct);
            CurrentPlayStatus = response.ToPlayStateInfo();
            return CurrentPlayStatus;
        }
        public async UniTask<IRoom> PlayGame(PlayGameConfig config, CancellationToken ct = default)
        {
            var info = await CanPlayGame(true, ct);
            if (info.PlayStatus != 0)
                throw new GameStatusException($"Can't start game. ErrorCode: {info.PlayStatus} Reason: {info.LabelInfo}");

            _joinedCustomMatchmakingData.Clear();

            if (config.CustomMatchmakingData != null)
                _joinedCustomMatchmakingData.AddRange(config.CustomMatchmakingData);

            CompetitivenessConfig? tournamentDetails = null;

            if (_tournamentCommunicator.CurrentTournament.HasValue)
                tournamentDetails = CompetitivenessConfig.GlobalTournament(_tournamentCommunicator.CurrentTournament.Value.Id);

            return await _roomsManager.StartQuickMatch(config.QueueName,
                config.GameEngineData,
                config.MatchmakerData,
                config.CustomRoomData,
                _joinedCustomMatchmakingData,
                competitivenessConfig: tournamentDetails,
                ct: ct);
        }

        public void OnWebMessage(WebMessage message)
        {
            var logger = _logger.WithMethodName();
            try
            {
                switch (message.type)
                {
                    case WebMessageTypes.PlayStatusUpdated:
                        var data = JsonUtility.FromJson<CanPlayUpdatedMessage>(message.message);
                        CurrentPlayStatus = data.ToPlayStateInfo();
                        PlayStatusUpdated?.Invoke(CurrentPlayStatus);
                        break;
                    default:
                        logger.LogError($"Unable to handle {message.type}");
                        break;
                }
            }
            catch (Exception e)
            {
                throw logger.LogExceptionAndReturn(e);
            }

        }

        private void SendSystemInfoData()
        {
            var joinedRoom = _lobby.RoomsManager.CurrentRoom;
            var systemInfoDataMessage = new SystemInfoDataMessage
            {
                userId = _lobby.AuthData!.UserId.ToString(),
                matchId = joinedRoom?.State.MatchmakingData?.MatchData?.MatchId.ToString() ?? string.Empty,
                systemInfoData = SystemInfoDataFactory.GetSystemInfoData(),
            };

            _playPadMessagingSystem.SendVoidMessage<SystemInfoDataMessage>(VoidMessageTypes.SystemInfoData, systemInfoDataMessage);
        }

        public void Dispose()
        {
            _playPadMessagingSystem.UnregisterIWebEventReceiver(this, WebMessageTypes.PlayStatusUpdated);
            _lobby.GameplaySceneMonitor.GameplayStarted -= SendSystemInfoData;
        }
        public void OnEvent(ElympicsStateChanged argument)
        {
            var message = new ElympicsStateUpdatedMessage
            {
                previousState = (int)argument.PreviousState,
                newState = (int)argument.NewState,
            };

            _playPadMessagingSystem.SendVoidMessage<ElympicsStateUpdatedMessage>(VoidMessageTypes.ElympicsStateUpdated, message);
        }
    }
}
