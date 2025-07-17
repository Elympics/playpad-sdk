#nullable enable

using System;

namespace ElympicsPlayPad.Protocol.Responses
{
    [Serializable]
    internal struct GetRollingTournamentDetailsResponse
    {
        public string state;
        public string[] prizes;
        public string coinId;
        public string entryFee;
        public int numberOfPlayers;
        public string gameVersion;
        public RollTournamentScore[] scores;

        [Serializable]
        public struct RollTournamentScore {
            public string state;
            public string avatar;
            public string nickname;
            public string? matchEnded;
            public bool mine;
            public float score;
            public uint position;
            public string prize;
        }
    }

}
