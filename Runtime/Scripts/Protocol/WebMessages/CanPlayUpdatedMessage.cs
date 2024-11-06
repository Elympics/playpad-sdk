using System;

namespace ElympicsPlayPad
{
    [Serializable]
    public struct CanPlayUpdatedMessage
    {
        public string status;
        public string labelMessage;
    }
}
