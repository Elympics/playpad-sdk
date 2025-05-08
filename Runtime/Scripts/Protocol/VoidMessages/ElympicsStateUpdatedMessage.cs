using System;

namespace ElympicsPlayPad.Protocol.VoidMessages
{
    [Serializable]
    public struct ElympicsStateUpdatedMessage
    {
        public int previousState;
        public int newState;
    }
}
