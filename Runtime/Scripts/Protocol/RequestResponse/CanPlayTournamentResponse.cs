using System;

namespace ElympicsPlayPad.Protocol.RequestResponse
{
    [Serializable]
    public struct CanPlayTournamentResponse
    {
        public int statusCode;
        public string message;
    }
}
