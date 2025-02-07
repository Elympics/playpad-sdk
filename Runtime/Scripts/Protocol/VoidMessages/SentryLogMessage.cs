using System;

namespace ElympicsPlayPad.Protocol.VoidMessages
{
    [Serializable]
    public struct SentryLogMessage
    {
        public int level;
        public string message;
    }
}
