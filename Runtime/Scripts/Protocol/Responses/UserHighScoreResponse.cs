using System;

namespace ElympicsPlayPad.Protocol.Responses
{
    [Serializable]
    public struct UserHighScoreResponse
    {
        public float points;
        public string endedAt;

    }
}
