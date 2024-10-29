using System;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.DTO;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.RequestResponse;
using ElympicsPlayPad.Tournament.Data;
using ElympicsPlayPad.Tournament.Extensions;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Tournament
{
    public class StandaloneTournamentCommunicator : IExternalTournamentCommunicator, IWebMessageReceiver
    {
        private readonly StandaloneExternalTournamentConfig _config;
        private readonly JsCommunicator _jsCommunicator;
        public StandaloneTournamentCommunicator(StandaloneExternalTournamentConfig config) => _config = config;

        internal StandaloneTournamentCommunicator(StandaloneExternalTournamentConfig config, JsCommunicator jsCommunicator)
        {
            _config = config;
            jsCommunicator.RegisterIWebEventReceiver(this, WebMessageTypes.TournamentUpdated);
        }
        public event Action<TournamentInfo> TournamentUpdated;
        public UniTask<CanPlayTournamentResponse> CanPlayTournament() => UniTask.FromResult(new CanPlayTournamentResponse()
        {
            statusCode = _config.CanPlayStatusCode,
            message = _config.CanPlayMessage
        });
        public void OnWebMessage(WebMessageObject message)
        {
            if (string.Equals(message.type, WebMessageTypes.TournamentUpdated) is false)
                throw new Exception($"{nameof(WebGLTournamentCommunicator)} can handle only {WebMessageTypes.TournamentUpdated} event type.");

            var newTournamentData = JsonUtility.FromJson<TournamentDataDto>(message.message);
            var tournamentInfo = newTournamentData?.ToTournamentInfo();
            if (tournamentInfo != null)
                TournamentUpdated?.Invoke(tournamentInfo.Value);
        }
    }
}
