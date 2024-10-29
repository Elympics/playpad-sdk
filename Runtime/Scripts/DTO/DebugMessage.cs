using System;

namespace ElympicsPlayPad.DTO
{
    [Serializable]
    public class DebugMessage<T>
    {
        public string debugType;
        public T message;
    }
}
