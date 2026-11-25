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
        public ushort heartbeatIntervalMs;
        public UserPrefs userPrefs;
        public int launchMode;
        public string gameVersionId;
        public string fleetName;
        /// <summary>The host's game mode. Missing, empty or "standard" means the online flow; any other value is an offline mode.</summary>
        public string? gameMode;
    }

    [Serializable]
    public struct UserPrefs
    {
        public string[] languages;
    }
}
