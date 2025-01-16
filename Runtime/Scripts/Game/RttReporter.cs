using System;
using Elympics;
using ElympicsPlayPad.ExternalCommunicators;
using UnityEngine;

namespace ElympicsPlayPad.Game
{
    [Obsolete("This component is no longer needed to report RTT. This feature is now integrated into PlayPad SDK and always enabled.", true)]
    public class RttReporter : MonoBehaviour, IClientHandlerGuid
    {
        [SerializeField] private int tickInterval;
        private ElympicsClient _client;
        private int _counter;
        private bool _sendRttUpdates;
        private void Start()
        {
            _sendRttUpdates = true;
            _client = FindObjectOfType<ElympicsClient>();
        }
        // public void Update()
        // {
        //     if (_client == null)
        //         return;
        //
        //     if (_sendRttUpdates is false)
        //         return;
        //
        //     ++_counter;
        //     if (_counter <= tickInterval)
        //         return;
        //
        //     _counter = 0;
        //     var avgRtt = _client.RoundTripTimeCalculator?.AverageRoundTripTime ?? TimeSpan.Zero;
        //     if (PlayPadCommunicator.Instance != null)
        //         PlayPadCommunicator.Instance.GameStatusCommunicator?.RttUpdated(avgRtt);
        // }

        public void OnConnectingFailed()
        {
            _sendRttUpdates = false;
            if (PlayPadCommunicator.Instance != null)
                PlayPadCommunicator.Instance.GameStatusCommunicator?.RttUpdated(TimeSpan.Zero);
        }
        public void OnDisconnectedByServer()
        {
            _sendRttUpdates = false;
            if (PlayPadCommunicator.Instance != null)
                PlayPadCommunicator.Instance.GameStatusCommunicator?.RttUpdated(TimeSpan.Zero);
        }
        public void OnDisconnectedByClient()
        {
            _sendRttUpdates = false;
            if (PlayPadCommunicator.Instance != null)
                PlayPadCommunicator.Instance.GameStatusCommunicator?.RttUpdated(TimeSpan.Zero);
        }

        private void OnDestroy()
        {
            _sendRttUpdates = false;
            if (PlayPadCommunicator.Instance != null)
                PlayPadCommunicator.Instance.GameStatusCommunicator?.RttUpdated(TimeSpan.Zero);
        }
    }
}
