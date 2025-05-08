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
        private readonly JsCommunicator _jsCommunicator;

        /// <param name="rttBufferSize">Number of calls to <see cref="OnRttReceived(RttReceived)"/> after which <see cref="FlushRttBuffer"/> will be called automatically.</param>
        /// <param name="jsCommunicator">Used to send collected data to PlayPad.</param>
        public WebGLRoundTripTimeReporter(int rttBufferSize, JsCommunicator jsCommunicator)
        {
            _rttBufferSize = rttBufferSize;
            _rttBuffer = new List<RttReceived>(rttBufferSize);
            _jsCommunicator = jsCommunicator;
        }

        public void OnRttReceived(RttReceived value)
        {
            _rttBuffer.Add(value);

            if (_rttBuffer.Count >= _rttBufferSize)
                FlushRttBuffer();
        }

        public void FlushRttBuffer()
        {
            var message = new NetworkStatusMessage { matchId = ElympicsLobbyClient.Instance!.MatchDataGuid!.MatchId.ToString(), data = _rttBuffer };
            _jsCommunicator.SendVoidMessage<NetworkStatusMessage>(VoidMessageTypes.NetworkStatusMessage, message);
            _rttBuffer.Clear();
        }
    }
}
