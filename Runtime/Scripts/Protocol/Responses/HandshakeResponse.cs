#nullable enable
using System;

namespace ElympicsPlayPad.Protocol.Responses
{
    [Serializable]
    public struct HandshakeResponse
    {
        public string? error;
        public string device;
        public string environment;
        public int capabilities;
        public int featureAccess;
        public string closestRegion;
    }
}
