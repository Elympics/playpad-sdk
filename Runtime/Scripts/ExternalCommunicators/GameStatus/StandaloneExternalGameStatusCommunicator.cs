using System;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.GameStatus
{
    public class StandaloneExternalGameStatusCommunicator : IExternalGameStatusCommunicator
    {
        public void GameFinished(int score) => Debug.Log($"Game Finished {score}");
        public void RttUpdated(TimeSpan rtt) => Debug.Log($"RttUpdated {rtt}");
        public void ApplicationInitialized() => Debug.Log($"Application Initialized.");
        public void Dispose()
        {
        }
    }
}
