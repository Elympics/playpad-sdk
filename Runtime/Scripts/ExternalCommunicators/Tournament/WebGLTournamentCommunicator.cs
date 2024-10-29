#nullable enable
using System;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Responses;
using ElympicsPlayPad.Protocol.WebMessages;
using ElympicsPlayPad.Tournament.Data;
using ElympicsPlayPad.Tournament.Extensions;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Tournament
{
    internal class WebGLTournamentCommunicator : IExternalTournamentCommunicator, IWebMessageReceiver
    {
        public TournamentInfo? CurrentTournament => _currentTournament;

        private TournamentInfo? _currentTournament;
        public event Action<TournamentInfo>? TournamentUpdated;
        public async UniTask<TournamentInfo?> GetTournament()
        {
            var response = await _jsCommunicator.SendRequestMessage<EmptyPayload, TournamentResponse>(ReturnEventTypes.GetTournament);
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
            if (string.Equals(message.type, WebMessageTypes.TournamentUpdated) is false)
                throw new Exception($"{nameof(WebGLTournamentCommunicator)} can handle only {WebMessageTypes.TournamentUpdated} event type.");

            var newTournamentData = JsonUtility.FromJson<TournamentResponse>(message.message);
            _currentTournament = newTournamentData.ToTournamentInfo();
            TournamentUpdated?.Invoke(_currentTournament.Value);
        }
    }
}
