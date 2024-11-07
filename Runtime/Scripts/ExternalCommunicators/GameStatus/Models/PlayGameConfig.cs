#nullable enable
using System.Collections.Generic;
using JetBrains.Annotations;

namespace ElympicsPlayPad.ExternalCommunicators.GameStatus.Models
{
    [PublicAPI]
    public struct PlayGameConfig
    {
        public string QueueName;
        public byte[]? GameEngineData;
        public float[]? MatchmakerData;
        public Dictionary<string, string>? CustomRoomData;
        public Dictionary<string, string>? CustomMatchmakingData;
    }
}
