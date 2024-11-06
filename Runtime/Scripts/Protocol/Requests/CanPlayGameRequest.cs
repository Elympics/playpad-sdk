using System;

namespace ElympicsPlayPad.Protocol.Requests
{
    [Serializable]
    public struct CanPlayGameRequest
    {
        public bool autoResolve;
    }
}
