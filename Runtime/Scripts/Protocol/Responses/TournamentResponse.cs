using System;

namespace ElympicsPlayPad.Protocol.Responses
{
    [Serializable]
    public struct TournamentResponse
    {
        public string id;
        public int leaderboardCapacity;
        public string name;
        public string ownerId;
        public string startDate;
        public string endDate;
        public bool isDefault;
    }
}
