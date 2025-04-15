using System;
namespace ElympicsPlayPad.Protocol.Requests
{
    [Serializable]
    public struct EnsureVirtualDepositRequest
    {
        public string amount;
        public string coinId;
    }
}
