#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Elympics;
using ElympicsPlayPad.ExternalCommunicators.GameStatus.Models;
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
        public PlayStatusInfo CurrentPlayStatus { get; }

        private PlayStatusInfo _currentPlayStatusInfo;
        public event Action<PlayStatusInfo>? PlayStatusUpdated;
        private readonly JsCommunicator _communicator;
        private readonly IElympicsLobbyWrapper _lobby;

        public WebGLGameStatusCommunicator(JsCommunicator communicator, IElympicsLobbyWrapper lobby)
        {
            _communicator = communicator;
            _communicator.RegisterIWebEventReceiver(this, WebMessageTypes.PlayStatusUpdated);

            _lobby = lobby;
            _lobby.GameplaySceneMonitor.GameplayStarted += SendSystemInfoData;
            _lobby.ElympicsStateUpdated += OnElympicsStateUpdated;
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

        public async UniTask<PlayStatusInfo> CanPlayGame(bool autoResolve)
        {
            var request = new CanPlayGameRequest
            {
                autoResolve = autoResolve
            };
            var response = await _communicator.SendRequestMessage<CanPlayGameRequest, CanPlayGameResponse>(ReturnEventTypes.GetPlayStatus, request);
            _currentPlayStatusInfo = response.ToPlayStateInfo();
            return _currentPlayStatusInfo;
        }

        public void RttUpdated(TimeSpan rtt) => _communicator.SendDebugMessage<RttDebugMessage>(DebugMessageTypes.RTT,
            new RttDebugMessage()
            {
                rtt = rtt.TotalMilliseconds
            });

        public void OnWebMessage(WebMessage message)
        {
            if (message.type != WebMessageTypes.PlayStatusUpdated)
                throw new Exception($"{nameof(WebGLGameStatusCommunicator)} can't handle {message.type} event type.");

            var data = JsonUtility.FromJson<CanPlayUpdatedMessage>(message.message);

            _currentPlayStatusInfo = data.ToPlayStateInfo();
            PlayStatusUpdated?.Invoke(_currentPlayStatusInfo);
        }

        private void SendSystemInfoData()
        {
            var joinedRooms = _lobby.RoomsManager.ListJoinedRooms();
            var systemInfoDataMessage = new SystemInfoDataMessage
            {
                userId = _lobby.AuthData.UserId.ToString(),
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
