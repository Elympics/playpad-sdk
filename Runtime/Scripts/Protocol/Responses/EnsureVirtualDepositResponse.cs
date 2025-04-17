using System;

namespace ElympicsPlayPad.Protocol.Responses
{
    [Serializable]
    public struct EnsureVirtualDepositResponse
    {
        public bool success;
        public string error;
    }
}
