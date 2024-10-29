using System;

namespace ElympicsPlayPad.Session.Exceptions
{
    public class WalletConnectionException : Exception
    {
        public WalletConnectionException(string message) : base(message)
        { }
    }
}
