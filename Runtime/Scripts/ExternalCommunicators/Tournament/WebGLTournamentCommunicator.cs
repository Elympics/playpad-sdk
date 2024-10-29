#nullable enable
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
    internal class WebGLTournamentCommunicator : IExternalTournamentCommunicator, IWebMessageReceiver
    {
        public event Action<TournamentInfo>? TournamentUpdated;
        public async UniTask<CanPlayTournamentResponse> CanPlayTournament() => await _jsCommunicator.SendRequestMessage<string, CanPlayTournamentResponse>(ReturnEventTypes.CanPlayTournament, string.Empty);

        private readonly JsCommunicator _jsCommunicator;
        public WebGLTournamentCommunicator(JsCommunicator jsCommunicator)
        {
            _jsCommunicator = jsCommunicator;
            _jsCommunicator.RegisterIWebEventReceiver(this, WebMessageTypes.TournamentUpdated);
        }
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
