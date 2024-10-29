using System;

namespace ElympicsPlayPad.Session.Exceptions
{
    public class SessionManagerAuthException : Exception
    {
        public SessionManagerAuthException(string message) : base(message)
        {
        }
    }
}
