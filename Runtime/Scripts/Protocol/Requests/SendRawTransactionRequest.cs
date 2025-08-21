using System;

namespace ElympicsPlayPad.Protocol.Requests
{
    [Serializable]
    internal struct SendRawTransactionRequest<T>
        where T : ISendRawTransactionPayload
    {
        public string type;
        public string destinationAddress;
        public string amount;
        public T payload;
    }

    internal interface ISendRawTransactionPayload
    { }

    [Serializable]
    internal struct EvmPayload : ISendRawTransactionPayload
    {
        public string chainId;
        public string data;
    }

    [Serializable]
    public struct TonPayload : ISendRawTransactionPayload
    {
        public string stateInit;
        public string payload;
    }
}
