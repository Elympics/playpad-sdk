using System;

namespace ElympicsPlayPad.Protocol.VoidMessages.DebugMessages
{
    [Serializable]
    public class DebugMessage<T>
    {
        public string debugType;
        public T message;
    }
}
