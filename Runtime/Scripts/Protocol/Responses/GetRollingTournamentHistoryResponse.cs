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
            public string[] prizes;
            public string coinId;
            public string entryFee;
            public int numberOfPlayers;
            public string gameVersion;
            public RollingTournamentScore[] scores;
            public bool unreadSettled;
        }
    }
}
