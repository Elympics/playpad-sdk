using System;
using Elympics.Communication.Authentication.Models.Internal;

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
        public int position;
        public float score;
        public string scoredAt;
        public string matchId;
        public string tournamentId;
        public ElympicsUserDTO user;
    }
}
