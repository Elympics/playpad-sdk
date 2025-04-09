#nullable enable

using System;
using System.Collections.Generic;
using Elympics.AssemblyCommunicator.Events;

namespace ElympicsPlayPad.Protocol.WebMessages
{
    [Serializable]
    internal struct NetworkStatusMessage
    {
        public string matchId;
        public List<RttReceived> data;
    }
}
