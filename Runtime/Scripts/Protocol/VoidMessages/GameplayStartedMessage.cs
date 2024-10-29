using System;
using ElympicsPlayPad.DTO;

namespace ElympicsPlayPad.Protocol.VoidMessages
{
    [Serializable]
    public struct GameplayStartedMessage
    {
        public string matchId;
        public SystemInfoData systemInfo;
    }
}
