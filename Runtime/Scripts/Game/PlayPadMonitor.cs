using System;
using Elympics;
using ElympicsPlayPad.ExternalCommunicators;
using UnityEngine;

namespace ElympicsPlayPad.Game
{
    public class PlayPadMonitor : MonoBehaviour, IClientHandlerGuid
    {
        /// <summary>Number of frames to wait before reporting new RTT.</summary>
        private const int TickInterval = 20;

        private ElympicsClient _client;
        private int _counter;

        private void Start()
        {
            ElympicsLobbyClient.Instance.GameplaySceneMonitor.GameplayStarted += StartSendingUpdates;
            ElympicsLobbyClient.Instance.GameplaySceneMonitor.GameplayFinished += StopSendingUpdates;
        }

        private void StartSendingUpdates()
        {
            ElympicsLogger.Log($"[KD] RTT Update - starting updates.");
            _client = FindObjectOfType<ElympicsClient>();

            if (_client == null)
                ElympicsLogger.LogError($"{nameof(ElympicsClient)} not found.");
        }

        private void StopSendingUpdates()
        {
            ElympicsLogger.Log($"[KD] RTT Update - stopping updates.");
            _client = null;
        }

        public void Update()
        {
            if (_client == null)
            {
                ElympicsLogger.Log($"[KD] RTT Update - client is null.");
                return;
            }

            ++_counter;
            if (_counter <= TickInterval)
                return;

            _counter = 0;
            var avgRtt = _client.RoundTripTimeCalculator?.AverageRoundTripTime ?? TimeSpan.Zero;
            var rttStdDev = _client.RoundTripTimeCalculator?.RoundTripTimeStandardDeviation ?? TimeSpan.Zero;
            if (PlayPadCommunicator.Instance != null && PlayPadCommunicator.Instance.GameStatusCommunicator != null)
            {
                PlayPadCommunicator.Instance.GameStatusCommunicator.RttUpdated(avgRtt);
                ElympicsLogger.Log($"[KD] RTT Update - update sent, RTT Avg: {avgRtt.TotalMilliseconds} ms StdDev: {rttStdDev.TotalMilliseconds} ms.");
            }
            else if (PlayPadCommunicator.Instance == null)
            {
                ElympicsLogger.Log($"[KD] RTT Update - PlayPadCommunicator.Instance is null.");
            }
            else
            {
                ElympicsLogger.Log($"[KD] RTT Update - GameStatusCommunicator is null.");
            }
        }

        public void OnConnectingFailed() => ReportFinalRtt();
        public void OnDisconnectedByServer() => ReportFinalRtt();
        public void OnDisconnectedByClient() => ReportFinalRtt();

        private void OnDestroy() => ReportFinalRtt();

        private void ReportFinalRtt()
        {
            StopSendingUpdates();
            if (PlayPadCommunicator.Instance != null)
                PlayPadCommunicator.Instance.GameStatusCommunicator?.RttUpdated(TimeSpan.Zero);
        }
    }
}
