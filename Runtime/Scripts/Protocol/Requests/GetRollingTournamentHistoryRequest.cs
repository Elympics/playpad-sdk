using System;

namespace ElympicsPlayPad.Protocol.Requests
{
    [Serializable]
    internal struct GetRollingTournamentHistoryRequest
    {
        public uint skip;
        public uint take;
    }
}
