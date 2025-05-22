using System;
namespace ElympicsPlayPad.Protocol.Responses
{
    [Serializable]
    public struct ElympicsCoinsResponse
    {
        public CurrencyResponse[] currencies;
    }

    [Serializable]
    public struct BalanceResponse
    {
        public string amount;
        public string coinId;
        public string error;
    }
}
