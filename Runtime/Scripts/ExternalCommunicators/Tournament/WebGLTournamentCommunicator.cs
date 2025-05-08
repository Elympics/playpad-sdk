#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics.ElympicsSystems.Internal;
using ElympicsPlayPad.ExternalCommunicators.Authentication;
using ElympicsPlayPad.ExternalCommunicators.Tournament.Extensions;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Responses;
using ElympicsPlayPad.Protocol.WebMessages;
using ElympicsPlayPad.Tournament.Data;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Tournament
{
    internal class WebGLTournamentCommunicator : IExternalTournamentCommunicator, IWebMessageReceiver
    {
        public event Action<TournamentInfo>? TournamentUpdated;
        public TournamentInfo? CurrentTournament => _currentTournament;

        private TournamentInfo? _currentTournament;

        private ElympicsLoggerContext _logger;
        public WebGLTournamentCommunicator(ElympicsLoggerContext logger) => _logger = logger.WithContext(nameof(WebGLTournamentCommunicator));

        public async UniTask<TournamentInfo?> GetTournament(CancellationToken ct = default)
        {
            var response = await _jsCommunicator.SendRequestMessage<EmptyPayload, TournamentResponse>(RequestResponseMessageTypes.GetTournament, null, ct);
            _currentTournament = response.ToTournamentInfo();
            return _currentTournament.Value;
        }
        private readonly JsCommunicator _jsCommunicator;
        public WebGLTournamentCommunicator(JsCommunicator jsCommunicator)
        {
            _jsCommunicator = jsCommunicator;
            _jsCommunicator.RegisterIWebEventReceiver(this, WebMessageTypes.TournamentUpdated);
        }
        public void OnWebMessage(WebMessage message)
        {
            var logger = _logger.WithMethodName();
            if (string.Equals(message.type, WebMessageTypes.TournamentUpdated) is false)
                throw logger.CaptureAndThrow(new Exception($"{nameof(WebGLTournamentCommunicator)} can handle only {WebMessageTypes.TournamentUpdated} event type."));
            try
            {
                switch (message.type)
                {
                    case WebMessageTypes.TournamentUpdated:
                        var newTournamentData = JsonUtility.FromJson<TournamentUpdatedMessage>(message.message);
                        _currentTournament = newTournamentData.ToTournamentInfo();
                        TournamentUpdated?.Invoke(_currentTournament.Value);
                        break;
                    default:
                        logger.Error($"Unable to handle message {message.type}");
                        break;
                }

            }
            catch (Exception e)
            {
                throw logger.CaptureAndThrow(e);
            }
        }
    }
}
