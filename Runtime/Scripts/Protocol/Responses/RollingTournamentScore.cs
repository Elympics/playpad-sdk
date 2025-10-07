#nullable enable
using System;
using Elympics.Communication.Authentication.Models.Internal;

namespace ElympicsPlayPad.Protocol.Responses
{
    [Serializable]
    internal struct RollingTournamentScore
    {
        public string state;
        public string? matchEnded;
        public bool mine;
        public float score;
        public uint position;
        public string prize;
        public ElympicsUserDTO user;
    }
}
