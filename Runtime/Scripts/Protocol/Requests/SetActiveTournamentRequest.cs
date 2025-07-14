using System;

namespace ElympicsPlayPad.Protocol.Requests
{
    [Serializable]
    internal struct SetActiveTournamentRequest
    {
        public string tournamentId;
    }
}
