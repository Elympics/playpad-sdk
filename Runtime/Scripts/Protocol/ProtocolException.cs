using System;

namespace ElympicsPlayPad.Protocol
{
    public class ProtocolException : Exception
    {
        public ProtocolException(string message, string messageType) : base($"[{messageType}] {message}")
        {

        }
    }
}
