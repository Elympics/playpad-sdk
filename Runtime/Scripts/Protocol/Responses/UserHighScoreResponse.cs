using System;

namespace ElympicsPlayPad.Protocol.Responses
{
    [Serializable]
    public struct UserHighScoreResponse
    {
        public string points;
        public string endedAt;

    }
}
