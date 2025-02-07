#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.ElympicsSystems.Internal;
using Elympics.Rooms.Models;
using ElympicsPlayPad.ExternalCommunicators.GameStatus.Exceptions;
using ElympicsPlayPad.ExternalCommunicators.GameStatus.Models;
using ElympicsPlayPad.ExternalCommunicators.Tournament;
using ElympicsPlayPad.ExternalCommunicators.Tournament.Utility;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Requests;
using ElympicsPlayPad.Protocol.Responses;
using ElympicsPlayPad.Protocol.VoidMessages;
using ElympicsPlayPad.Protocol.VoidMessages.DebugMessages;
using ElympicsPlayPad.Protocol.WebMessages;
using ElympicsPlayPad.Utility;
using ElympicsPlayPad.Wrappers;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.GameStatus
{
    internal class WebGLGameStatusCommunicator : IExternalGameStatusCommunicator, IWebMessageReceiver
    {
        public PlayStatusInfo CurrentPlayStatus { get; private set; }
        public event Action<PlayStatusInfo>? PlayStatusUpdated;
        private readonly JsCommunicator _communicator;
        private readonly IElympicsLobbyWrapper _lobby;
        private readonly IExternalTournamentCommunicator _tournamentCommunicator;
        private readonly IRoomsManager _roomsManager;
        private readonly Dictionary<string, string> _joinedCustomMatchmakingData = new();
        private ElympicsLoggerContext _logger;

        public WebGLGameStatusCommunicator(JsCommunicator communicator, IElympicsLobbyWrapper lobby, IExternalTournamentCommunicator tournamentCommunicator, ElympicsLoggerContext logger)
        {
            _communicator = communicator;
            _communicator.RegisterIWebEventReceiver(this, WebMessageTypes.PlayStatusUpdated);
            _lobby = lobby;
            _tournamentCommunicator = tournamentCommunicator;
            _logger = logger;
            _lobby.GameplaySceneMonitor.GameplayStarted += SendSystemInfoData;
            _lobby.ElympicsStateUpdated += OnElympicsStateUpdated;
            _roomsManager = _lobby.RoomsManager;
            _logger = logger.WithContext(nameof(WebGLGameStatusCommunicator));
        }
        private void OnElympicsStateUpdated(ElympicsState previousState, ElympicsState newState)
        {
            var message = new ElympicsStateUpdatedMessage
            {
                previousState = (int)previousState,
                newState = (int)newState
            };

            _communicator.SendVoidMessage<ElympicsStateUpdatedMessage>(VoidEventTypes.ElympicsStateUpdated, message);
        }

        public void HideSplashScreen() => _communicator.SendVoidMessage<EmptyPayload>(VoidEventTypes.HideSplashScreen);

        public async UniTask<PlayStatusInfo> CanPlayGame(bool autoResolve, CancellationToken ct = default)
        {
            var request = new CanPlayGameRequest
            {
                autoResolve = autoResolve
            };
            var response = await _communicator.SendRequestMessage<CanPlayGameRequest, CanPlayGameResponse>(ReturnEventTypes.GetPlayStatus, request, ct);
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

            if (_tournamentCommunicator.CurrentTournament.HasValue)
                _joinedCustomMatchmakingData[TournamentConst.TournamentIdKey] = _tournamentCommunicator.CurrentTournament.Value.Id;
            else
                _joinedCustomMatchmakingData.Remove(TournamentConst.TournamentIdKey);


            return await _roomsManager.StartQuickMatch(config.QueueName, config.GameEngineData, config.MatchmakerData, config.CustomRoomData, _joinedCustomMatchmakingData, ct);
        }

        public void RttUpdated(TimeSpan rtt) => _communicator.SendDebugMessage<RttDebugMessage>(DebugMessageTypes.RTT,
            new RttDebugMessage()
            {
                rtt = rtt.TotalMilliseconds
            });

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
                        logger.Error($"Unable to handle {message.type}");
                        break;
                }
            }
            catch (Exception e)
            {
                throw logger.CaptureAndThrow(e);
            }

        }

        private void SendSystemInfoData()
        {
            var joinedRooms = _lobby.RoomsManager.ListJoinedRooms();
            var systemInfoDataMessage = new SystemInfoDataMessage
            {
                userId = _lobby.AuthData!.UserId.ToString(),
                matchId = joinedRooms.Count > 0 ? (joinedRooms[0].State.MatchmakingData?.MatchData?.MatchId.ToString() ?? string.Empty) : string.Empty,
                systemInfoData = SystemInfoDataFactory.GetSystemInfoData()
            };

            _communicator.SendVoidMessage<SystemInfoDataMessage>(VoidEventTypes.SystemInfoData, systemInfoDataMessage);
        }

        public void Dispose()
        {
            _lobby.GameplaySceneMonitor.GameplayStarted -= SendSystemInfoData;
            _lobby.ElympicsStateUpdated -= OnElympicsStateUpdated;
        }
    }
}
