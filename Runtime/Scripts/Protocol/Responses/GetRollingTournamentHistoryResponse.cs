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
            public RollingTournamentScore myScore;
            public RollingTournamentScore[] allScores;
            public bool unreadSettled;
        }

        [Serializable]
        public struct Tournament
        {
            public string[] prizes;
            public string coinId;
            public string entryFee;
            public int numberOfPlayers;
            public string gameVersion;
        }
    }
}
