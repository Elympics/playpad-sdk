using System;

namespace ElympicsPlayPad.Protocol.WebMessages
{
    [Serializable]
    public struct UserHighScoreUpdatedMessage
    {
        public float points;
        public string endedAt;
    }
}
