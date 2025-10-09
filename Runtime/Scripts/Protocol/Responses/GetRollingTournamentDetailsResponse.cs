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
        public RollingTournamentScore[] scores;
    }

}
