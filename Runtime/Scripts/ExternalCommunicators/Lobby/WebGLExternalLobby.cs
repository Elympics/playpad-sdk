#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.ElympicsSystems.Internal;
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
        private readonly ElympicsLoggerContext _logger;

        public WebGLExternalLobby(PlayPadMessagingSystem playPadMessagingSystem, ElympicsLoggerContext logger)
        {
            Lobby = new LobbyInfo
            {
                IsMatchReady = false,
                MatchData = null
            };
            _playPadMessagingSystem = playPadMessagingSystem;
            _logger = logger.WithContext(nameof(WebGLExternalLobby));
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
                    _ = logger.SetMatchId(Lobby.MatchData.MatchId.ToString());
                else
                    _ = logger.SetNoRoom();
                logger.Log("Lobby status recieved.");
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
            _ = _logger.SetMatchId(Lobby.MatchData!.MatchId.ToString()).SetQueue(Lobby.MatchData.QueueName)
                .SetServerAddress(Lobby.MatchData.WebServerAddress, Lobby.MatchData.TcpUdpServerAddress);
            _logger.WithMethodName().Log("Play Match.");
            LobbyRegister.PlayMatchInternal(Lobby.MatchData);
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
                _ = logger.SetMatchId(Lobby.MatchData.MatchId.ToString());
            else
                _ = logger.SetNoRoom();
            logger.Log("Handling lobby status update.");
            OnLobbyInfoUpdated?.Invoke(Lobby);
        }
    }
}
