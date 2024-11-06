using System;

namespace ElympicsPlayPad.Protocol.WebMessages
{
    [Serializable]
    public class TournamentUpdatedMessage
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
