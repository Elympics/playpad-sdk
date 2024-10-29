using System;

namespace ElympicsPlayPad.Protocol.Requests
{
    [Serializable]
    public struct SignTypedDataRequest
    {
        public string address;
        public string dataToSign;
    }
}
