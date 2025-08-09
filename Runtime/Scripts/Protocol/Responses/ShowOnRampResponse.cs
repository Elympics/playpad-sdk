using System;

namespace ElympicsPlayPad.Protocol.Responses
{
    [Serializable]
    public struct ShowOnRampResponse
    {
        public bool success;
        public string error;
    }
}
