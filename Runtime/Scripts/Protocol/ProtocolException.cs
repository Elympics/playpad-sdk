using Elympics;

namespace ElympicsPlayPad.Protocol
{
    public class ProtocolException : ElympicsException
    {
        public ProtocolException(string message, string messageType) : base($"[{messageType}] {message}")
        {

        }
    }
}
