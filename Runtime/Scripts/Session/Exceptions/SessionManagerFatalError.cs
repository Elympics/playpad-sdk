using System;

namespace ElympicsPlayPad.Session.Exceptions
{
    public class SessionManagerFatalError : Exception
    {
        public SessionManagerFatalError(string reason) : base($"Session manager fatal error. Reason: {reason}")
        {

        }
    }
}
