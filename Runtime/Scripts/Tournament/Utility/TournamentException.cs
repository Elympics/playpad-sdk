using System;

namespace ElympicsPlayPad.Tournament.Utility
{
    public class TournamentException : Exception
    {
        public TournamentException(string message) : base(message)
        {
        }
    }
}
