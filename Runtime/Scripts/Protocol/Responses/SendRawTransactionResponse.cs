using System;

namespace ElympicsPlayPad.Protocol.Responses
{
    [Serializable]
    public struct SendRawTransactionResponse
    {
        public string txHash;
        public string error;
    }
}
