using System;
using Elympics;
using UnityEngine;

namespace ElympicsPlayPad.Game
{
    [Obsolete("This component is no longer needed to report RTT. This feature is now integrated into PlayPad SDK and always enabled.", true)]
    public class RttReporter : MonoBehaviour, IClientHandlerGuid
    {
        public void OnConnectingFailed() { }
        public void OnDisconnectedByServer() { }
        public void OnDisconnectedByClient() { }

        private void OnDestroy() { }
    }
}
