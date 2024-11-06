using System;

namespace ElympicsPlayPad.Protocol.WebMessages
{
    [Serializable]
    public struct ElympicsStateUpdatedMessage
    {
        public int previousState;
        public int newState;
    }
}
