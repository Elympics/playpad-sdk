using System;
namespace ElympicsPlayPad.Protocol.Responses
{
    [Serializable]
    internal struct LobbyStatusResponse
    {
        public string matchId;
        public string userSecret;
        public string queueName;
        public string regionName;
        public byte[] gameEngineData;
        public float[] matchmakerData;
        public string tcpUdpServerAddress;
        public string webServerAddress;
        public string[] matchedPlayers;
    }
}
