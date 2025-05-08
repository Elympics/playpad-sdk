using System;

namespace ElympicsPlayPad.Protocol.WebMessages
{
    [Serializable]
    public struct CanPlayUpdatedMessage
    {
        public string status;
        public string labelMessage;
        public bool isHintAvailable;
    }
}
