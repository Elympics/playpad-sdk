using System;

namespace ElympicsPlayPad.Protocol.Responses
{
    [Serializable]
    internal struct GetRollingTournamentHistoryResponse
    {
        public HistoryEntry[] entries;

        [Serializable]
        public struct HistoryEntry
        {
            public string state;
            public Tournament tournament;
            public Participation myScore;
            public Participation[] allScores;
            public bool unreadSettled;
        }

        [Serializable]
        public struct Tournament
        {
            public string prize;
            public string coinId;
            public string entryFee;
            public int numberOfPlayers;
            public string gameVersion;
        }

        [Serializable]
        public struct Participation
        {
            public string avatar;
            public string nickname;
            public string matchEnded;
            public float score;
            public int position;
        }
    }
}
