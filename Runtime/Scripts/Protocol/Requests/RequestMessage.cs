using System;

namespace ElympicsPlayPad.Protocol.Requests
{
    [Serializable]
    public class RequestMessage<T>
    {
        public int ticket;
        public string type;
        public T payload;
    }
}
