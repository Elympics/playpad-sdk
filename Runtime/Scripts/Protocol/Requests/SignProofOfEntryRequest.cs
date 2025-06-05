using System;

namespace ElympicsPlayPad.Protocol.Requests
{
    [Serializable]
    internal struct SignProofOfEntryRequest
    {
        public string coinId;
        public string amount;
        public string roomId;
    }
}
