#nullable enable

using System;
using Elympics.AssemblyCommunicator.Events;

namespace ElympicsPlayPad.Protocol.WebMessages
{
    [Serializable]
    internal struct NetworkStatusMessage
    {
        public string matchId;
        public RttReceived[] data;
    }
}
