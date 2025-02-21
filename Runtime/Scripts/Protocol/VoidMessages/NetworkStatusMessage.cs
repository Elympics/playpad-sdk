#nullable enable

using System;

namespace ElympicsPlayPad.Protocol.WebMessages
{
    [Serializable]
    internal struct NetworkStatusMessage
    {
        public string matchId;
        public byte[] serializedData;
    }
}
