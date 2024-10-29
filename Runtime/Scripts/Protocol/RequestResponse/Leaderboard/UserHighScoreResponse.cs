using System;

namespace ElympicsPlayPad.Protocol.RequestResponse.Leaderboard
{
    [Serializable]
    public class UserHighScoreResponse
    {
        public float points;
        public string endedAt;
    }
}
