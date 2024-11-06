using System;

namespace ElympicsPlayPad.Protocol.WebMessages
{
    [Serializable]
    public struct UserHighScoreUpdatedMessage
    {
        public float score;
    }
}
