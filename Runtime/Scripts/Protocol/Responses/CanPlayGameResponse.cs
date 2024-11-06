using System;

namespace ElympicsPlayPad.Protocol.Responses
{
    [Serializable]
    public struct CanPlayGameResponse
    {
        public string status;
        public string labelMessage;
    }
}
