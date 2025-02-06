using System;

namespace ElympicsPlayPad.Protocol.WebMessages
{
    [Serializable]
    public struct UserHighScoreUpdatedMessage
    {
        public string points;
        public string endedAt;
    }
}
