using System;
namespace ElympicsPlayPad.Protocol.Responses
{
    [Serializable]
    public struct VirtualDepositResponse
    {
        public DepositResponse[] deposits;
    }

    [Serializable]
    public struct DepositResponse
    {
        public string amount;
        public CurrencyResponse currency;
    }

    [Serializable]
    public struct CurrencyResponse
    {
        public string coinId;
        public string ticker;
        public string address;
        public int decimals;
        public string iconUrl;
        public string chainType;
        public string chainName;
        public int chainExternalId;
    }
}
