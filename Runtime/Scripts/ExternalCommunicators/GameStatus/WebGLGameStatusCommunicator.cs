using System;
using System.Linq;
using ElympicsPlayPad.DTO;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.VoidMessages;
using ElympicsPlayPad.Utility;
using ElympicsPlayPad.Wrappers;

namespace ElympicsPlayPad.ExternalCommunicators.GameStatus
{
    internal class WebGLGameStatusCommunicator : IExternalGameStatusCommunicator
    {
        private readonly JsCommunicator _communicator;
        private readonly IElympicsLobbyWrapper _elympicsLobby;
        public WebGLGameStatusCommunicator(JsCommunicator communicator, IElympicsLobbyWrapper elympicsLobby)
        {
            _communicator = communicator;
            _elympicsLobby = elympicsLobby;
            _elympicsLobby.GameplaySceneMonitor.GameplayStarted += OnGameplayStarted;
            _elympicsLobby.GameplaySceneMonitor.GameplayFinished += OnGameplayFinished;
        }
        private void OnGameplayFinished() => _communicator.SendVoidMessage(VoidEventTypes.GameplayFinished, string.Empty);
        private void OnGameplayStarted()
        {
            var rooms = _elympicsLobby.RoomsManager.ListJoinedRooms();
            var matchRoom = rooms.FirstOrDefault(x => x.State.MatchmakingData?.MatchData is not null);
            var matchId = matchRoom?.State.MatchmakingData?.MatchData?.MatchId;
            if (matchId == null)
                return;
            var data = SystemInfoDataFactory.GetSystemInfoData();
            var voidData = new GameplayStartedMessage
            {
                matchId = matchId.ToString(),
                systemInfo = data
            };
            _communicator.SendVoidMessage(VoidEventTypes.GameplayStarted, voidData);
        }

        public void GameFinished(int score) => _communicator.SendVoidMessage(VoidEventTypes.GameFinished, score);
        public void RttUpdated(TimeSpan rtt)
        {
            var debugMessage = JsCommunicationFactory.GetDebugMessageJson(DebugMessageTypes.RTT,
                new RttDebugMessage()
                {
                    rtt = rtt.TotalMilliseconds
                });
            _communicator.SendVoidMessage(VoidEventTypes.Debug, debugMessage);
        }
        public void ApplicationInitialized() => _communicator.SendVoidMessage(VoidEventTypes.ApplicationInitialized, string.Empty);


        public void Dispose()
        {
            _elympicsLobby.GameplaySceneMonitor.GameplayStarted -= OnGameplayStarted;
            _elympicsLobby.GameplaySceneMonitor.GameplayFinished -= OnGameplayFinished;
        }
    }
}
