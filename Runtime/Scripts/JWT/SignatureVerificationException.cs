using System;

namespace ElympicsPlayPad.JWT
{
    public class SignatureVerificationException : Exception
    {
        public SignatureVerificationException(string message)
            : base(message)
        {
        }
    }
}
