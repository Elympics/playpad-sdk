using System;
namespace ElympicsPlayPad.Protocol.WebMessages
{
    [Serializable]
    public struct VirtualDepositUpdatedMessage
    {
        public DepositUpdated[] deposits;
    }

    [Serializable]
    public struct DepositUpdated
    {
        public string amount;
        public CurrencyUpdated currency;
    }

    [Serializable]
    public struct CurrencyUpdated
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
