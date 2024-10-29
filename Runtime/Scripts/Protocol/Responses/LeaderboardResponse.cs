using System;
using UnityEngine.Serialization;

namespace ElympicsPlayPad.Protocol.Responses
{
    [Serializable]
    public struct LeaderboardResponse
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
