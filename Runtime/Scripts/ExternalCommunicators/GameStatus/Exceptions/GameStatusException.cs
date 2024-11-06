using System;

namespace ElympicsPlayPad.ExternalCommunicators.GameStatus.Exceptions
{
    public class GameStatusException : Exception
    {
        public GameStatusException(string message) : base(message)
        {
        }
    }
}
