using System;

namespace ElympicsPlayPad.Protocol.Requests
{
    [Serializable]
    public struct TransactionToSignRequest
    {
        public string from;
        public string to;
        public string data;
    }
}
