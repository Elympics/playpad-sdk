using System;

namespace ElympicsPlayPad.Protocol.WebMessages
{
    [Serializable]
    public struct TournamentUpdatedMessage
    {
        public string id;
        public int leaderboardCapacity;
        public PrizePoolMessage prizePool;
        public string name;
        public string ownerId;
        public string startDate;
        public string endDate;
        public bool isDefault;
    }

    [Serializable]
    public struct PrizePoolMessage
    {
        public string type;
        public string displayName;
        public byte[] image;
        public float amount;
        public string description;
    }
}
