#nullable enable

using System;
using System.Collections.Generic;
using Elympics;
using Elympics.Core.Utils;
using Elympics.AssemblyCommunicator.Events;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.WebMessages;

namespace ElympicsPlayPad.ExternalCommunicators.Sentry
{
    /// <summary>Collects data about RTT and sends it to PlayPad once enough data was collected.</summary>
    internal class WebGLRoundTripTimeReporter : IDisposable
    {
        private readonly int _rttBufferSize;
        private readonly List<RttReceived> _rttBuffer;
        private readonly JsCommunicator _jsCommunicator;
        private readonly NetworkStatusMessageFactory _networkStatusMessageFactory;

        /// <param name="rttBufferSize">Number of calls to <see cref="OnRttReceived(RttReceived)"/> after which <see cref="FlushRttBuffer"/> will be called automatically.</param>
        /// <param name="jsCommunicator">Used to send collected data to PlayPad.</param>
        public WebGLRoundTripTimeReporter(int rttBufferSize, JsCommunicator jsCommunicator)
        {
            _rttBufferSize = rttBufferSize;
            _rttBuffer = new(rttBufferSize);
            _jsCommunicator = jsCommunicator;
            _networkStatusMessageFactory = new NetworkStatusMessageFactory(rttBufferSize);
        }

        public void OnRttReceived(RttReceived value)
        {
            _rttBuffer.Add(value);

            if (_rttBuffer.Count >= _rttBufferSize)
                FlushRttBuffer();
        }

        /// <summary>Notify all <see cref="_reportReceivers"/> and clear collected data.</summary>
        public void FlushRttBuffer()
        {
            var message = _networkStatusMessageFactory.CreateMessage(ElympicsLobbyClient.Instance!.MatchDataGuid!.MatchId, _rttBuffer);
            _jsCommunicator.SendVoidMessage<NetworkStatusMessage>(VoidEventTypes.NetworkStatusMessage, message);
            _rttBuffer.Clear();
        }

        public void Dispose() => ((IDisposable)_networkStatusMessageFactory).Dispose();
    }
}
