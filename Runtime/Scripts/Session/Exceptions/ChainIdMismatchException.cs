using System;

namespace ElympicsPlayPad.Session.Exceptions
{
    public class ChainIdMismatch : Exception
    {
        public ChainIdMismatch(string chainId, string expected) : base($"Expecting {expected} chainId instead of {chainId}")
        {

        }
    }
}
