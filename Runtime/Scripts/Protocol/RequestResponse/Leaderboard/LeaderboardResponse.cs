using System;

namespace ElympicsPlayPad.Protocol.RequestResponse.Leaderboard
{
    [Serializable]
    internal class LeaderboardResponse
    {
        public Entry[] entries;
        public int totalRecords;
        public int pageNumber;
    }

    [Serializable]
    internal class Entry
    {
        public string userId;
        public string nickname;
        public int position;
        public float points;
        public string scoredAt;
        public string matchId;
        public string tournamentId;
    }
}
