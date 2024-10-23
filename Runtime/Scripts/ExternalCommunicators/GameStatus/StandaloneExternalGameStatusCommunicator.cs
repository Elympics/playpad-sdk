using System;
using ElympicsLobbyPackage.ExternalCommunication;
using UnityEngine;

namespace ElympicsLobbyPackage
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
