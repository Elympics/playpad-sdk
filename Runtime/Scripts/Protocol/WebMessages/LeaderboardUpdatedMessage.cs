using System;
namespace ElympicsPlayPad.Protocol.WebMessages
{
    [Serializable]
    public struct LeaderboardUpdatedMessage
    {
        public Entry[] entries;
        public Entry userEntry;
        public int participants;
    }

    [Serializable]
    public struct Entry
    {
        public string userId;
        public string nickname;
        public int position;
        public float score;
        public string scoredAt;
        public string matchId;
        public string tournamentId;
    }
}
