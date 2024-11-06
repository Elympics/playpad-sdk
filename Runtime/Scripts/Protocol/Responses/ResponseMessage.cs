using System;

namespace ElympicsPlayPad.Protocol.Responses
{
    [Serializable]
    public class ResponseMessage
    {
        public int ticket;
        public string type;
        public int status;
        public string response;
    }
}
