#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics.Core.Logger;
using ElympicsPlayPad.ExternalCommunicators.Lobby.Models;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Responses;
using ElympicsPlayPad.Protocol.VoidMessages;
using ElympicsPlayPad.Protocol.WebMessages;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Lobby
{
    internal class WebGLExternalLobby : IExternalLobbyCommunicator, IWebMessageReceiver
    {
        public event Action<LobbyInfo>? OnLobbyInfoUpdated;
        public LobbyInfo Lobby { get; private set; }
        private readonly PlayPadMessagingSystem _playPadMessagingSystem;
        private readonly PlayPadCommunicatorInternal _communicatorInternal;
        private readonly LoggerConfig _logger = ElympicsLogger.WithPlayPadSdkService()
            .WithClass(typeof(WebGLExternalLobby))
            .WithMonitoringEnabled();

        public WebGLExternalLobby(PlayPadMessagingSystem playPadMessagingSystem, PlayPadCommunicatorInternal communicatorInternal)
        {
            _communicatorInternal = communicatorInternal;
            Lobby = new LobbyInfo
            {
                IsMatchReady = false,
                MatchData = null
            };
            _playPadMessagingSystem = playPadMessagingSystem;
            _playPadMessagingSystem.RegisterIWebEventReceiver(this, WebMessageTypes.LobbyStatusUpdated);
        }

        public async UniTask<LobbyInfo> GetLobbyStatus(CancellationToken ct = default)
        {
            try
            {
                var result = await _playPadMessagingSystem.SendRequestMessage<EmptyPayload, LobbyStatusResponse>(RequestResponseMessageTypes.GetLobbyStatus, null, ct);
                Lobby = result.ToLobbyInfo();
                var logger = _logger.WithMethodName();
                if (Lobby.IsMatchReady)
                    ElympicsLogger.State.SetMatchId(Lobby.MatchData!.MatchId.ToString());
                else
                    ElympicsLogger.State.ClearRoom();
                logger.LogInfo("Lobby status recieved.");
                return Lobby;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while getting lobby status: {e}");
                throw;
            }
        }

        public void Quit(QuitArgs endGame)
        {
            var message = new QuitMessage
            {
                score = endGame.Score ?? -1,
            };
            _playPadMessagingSystem.SendVoidMessage<QuitMessage>(VoidMessageTypes.Quit, message);
        }

        public void PlayMatch()
        {
            if (!Lobby.IsMatchReady)
                throw new InvalidOperationException("Cannot play match: Lobby is null or match is not ready.");
            var logger = _logger.WithMethodName();
            logger.LogInfo("Play Match.");
            _communicatorInternal.PlayMatchInternal(Lobby.MatchData!).ContinueWith(result =>
            {
                if (!result.Success)
                    logger.LogError(result.ToString());
            }).Forget(e => logger.LogException(e));
        }

        public void OnWebMessage(WebMessage message)
        {
            switch (message.type)
            {
                case WebMessageTypes.LobbyStatusUpdated:
                    HandleLobbyStatusUpdated(message);
                    break;
                default:
                    break;
            }
        }

        private void HandleLobbyStatusUpdated(WebMessage message)
        {
            var logger = _logger.WithMethodName();
            var result = JsonUtility.FromJson<LobbyStatusResponse>(message.message);
            Lobby = result.ToLobbyInfo();
            if (Lobby.IsMatchReady)
                ElympicsLogger.State.SetMatchId(Lobby.MatchData!.MatchId.ToString());
            else
                ElympicsLogger.State.ClearRoom();
            logger.LogInfo("Handling lobby status update.");
            OnLobbyInfoUpdated?.Invoke(Lobby);
        }
    }
}
