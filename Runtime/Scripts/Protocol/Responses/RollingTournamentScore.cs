#nullable enable
using System;
namespace ElympicsPlayPad.Protocol.Responses
{
    [Serializable]
    internal struct RollingTournamentScore
    {
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
