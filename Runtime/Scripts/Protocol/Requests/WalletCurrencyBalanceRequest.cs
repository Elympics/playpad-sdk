using System;
namespace ElympicsPlayPad.Protocol.Requests
{
    [Serializable]
    internal struct WalletCurrencyBalanceRequest
    {
        public string coinId;
        public string walletAddress;
    }
}
