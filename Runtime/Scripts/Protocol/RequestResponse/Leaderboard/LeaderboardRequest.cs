using System;
using Elympics;

namespace ElympicsPlayPad.Protocol.RequestResponse.Leaderboard
{
    [Serializable]
    internal class LeaderboardRequest
    {
        public string tournamentId;
        public string queueName;
        public LeaderboardTimeScopeType timeScopeType;
        public string dateFrom;
        public string dateTo;
        public string fetchType;
        public int pageNumber;
        public int pageSize;
    }
}
