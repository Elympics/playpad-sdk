#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.AssemblyCommunicator;
using Elympics.AssemblyCommunicator.Events;
using Elympics.Core.Logger;
using ElympicsPlayPad.ExternalCommunicators.GameStatus.Models;
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
        private readonly ElympicsMatchLauncher _launcher;
        private readonly LoggerConfig _logger = ElympicsLogger.WithPlayPadSdkService()
            .WithClass(typeof(WebGLGameStatusCommunicator))
            .WithMonitoringEnabled();

        public WebGLGameStatusCommunicator(PlayPadMessagingSystem playPadMessagingSystem, ElympicsMatchLauncher launcher)
        {
            _playPadMessagingSystem = playPadMessagingSystem;
            _playPadMessagingSystem.RegisterIWebEventReceiver(this, WebMessageTypes.PlayStatusUpdated);
            _launcher = launcher;
            _launcher.StateChanged += OnLauncherStateChanged;
            CrossAssemblyEventBroadcaster.AddObserver(this);
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

        private void OnLauncherStateChanged(MatchLauncherState state)
        {
            if (state == MatchLauncherState.Running)
                SendSystemInfoData();
        }

        private void SendSystemInfoData()
        {
            var systemInfoDataMessage = new SystemInfoDataMessage
            {
                userId = LaunchedMatch.UserId(_launcher)?.ToString() ?? string.Empty,
                matchId = LaunchedMatch.MatchId(_launcher),
                systemInfoData = SystemInfoDataFactory.GetSystemInfoData(),
            };

            _playPadMessagingSystem.SendVoidMessage<SystemInfoDataMessage>(VoidMessageTypes.SystemInfoData, systemInfoDataMessage);
        }

        public void Dispose()
        {
            _playPadMessagingSystem.UnregisterIWebEventReceiver(this, WebMessageTypes.PlayStatusUpdated);
            _launcher.StateChanged -= OnLauncherStateChanged;
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
