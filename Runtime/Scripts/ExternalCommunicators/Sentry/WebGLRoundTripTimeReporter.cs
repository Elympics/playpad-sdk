#nullable enable
using System.Collections.Generic;
using Elympics;
using Elympics.AssemblyCommunicator.Events;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.VoidMessages;

namespace ElympicsPlayPad.ExternalCommunicators.Sentry
{
    /// <summary>Collects data about RTT and sends it to PlayPad once enough data was collected.</summary>
    internal class WebGLRoundTripTimeReporter
    {
        private readonly int _rttBufferSize;
        private readonly List<RttReceived> _rttBuffer;
        private readonly PlayPadMessagingSystem _playPadMessagingSystem;

        private ReceivedStatsUpdated _receivedStats;

        /// <param name="rttBufferSize">Number of calls to <see cref="OnRttReceived(RttReceived)"/> after which <see cref="FlushRttBuffer"/> will be called automatically.</param>
        /// <param name="playPadMessagingSystem">Used to send collected data to PlayPad.</param>
        public WebGLRoundTripTimeReporter(int rttBufferSize, PlayPadMessagingSystem playPadMessagingSystem)
        {
            _rttBufferSize = rttBufferSize;
            _rttBuffer = new List<RttReceived>(rttBufferSize);
            _playPadMessagingSystem = playPadMessagingSystem;
        }

        public void OnRttReceived(RttReceived value)
        {
            _rttBuffer.Add(value);

            if (_rttBuffer.Count >= _rttBufferSize)
                FlushRttBuffer();
        }

        public void OnReceivedStatsUpdated(ReceivedStatsUpdated stats) => _receivedStats = stats;

        public void FlushRttBuffer()
        {
            var message = new NetworkStatusMessage
            {
                matchId = LobbyRegister.GetMatchData()?.MatchId.ToString() ?? string.Empty,
                data = _rttBuffer,
                receivedStats = _receivedStats,
            };
            _playPadMessagingSystem.SendVoidMessage<NetworkStatusMessage>(VoidMessageTypes.NetworkStatusMessage, message);
            _rttBuffer.Clear();
        }
    }
}
