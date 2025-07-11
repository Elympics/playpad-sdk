using System;

namespace ElympicsPlayPad.Protocol.Responses
{
    [Serializable]
    internal struct ResultPayloadResponse
    {
        public bool success;
        public string error;
    }
}
