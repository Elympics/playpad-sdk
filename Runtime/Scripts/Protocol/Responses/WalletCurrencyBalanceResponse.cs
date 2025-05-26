using System;
namespace ElympicsPlayPad.Protocol.Responses
{
    [Serializable]
    internal struct WalletCurrencyBalanceResponse
    {
        public string amount;
        public string error;
    }
}
