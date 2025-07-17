using System;

namespace ElympicsPlayPad.Protocol.Requests
{
    [Serializable]
    internal struct GetRollingTournamentDetailsRequest
    {
        public string matchId;
    }
}
