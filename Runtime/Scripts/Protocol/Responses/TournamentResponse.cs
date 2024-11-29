using System;

namespace ElympicsPlayPad.Protocol.Responses
{
    [Serializable]
    public struct TournamentResponse
    {
        public string id;
        public int leaderboardCapacity;
        public string name;
        public PrizePoolResponse prizePool;
        public string ownerId;
        public string startDate;
        public string endDate;
        public bool isDefault;
    }

    [Serializable]
    public struct PrizePoolResponse
    {
        public string type;
        public string displayName;
        public string image;
        public float amount;
        public string description;
    }
}
