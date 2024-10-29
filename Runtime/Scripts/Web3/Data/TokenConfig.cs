#nullable enable

using System;

namespace ElympicsPlayPad.Web3.Data
{
    [Serializable]
    public struct TokenConfig
    {
        public Guid id;
        public int chainId;
        public int decimals;
        public string name;
        public string symbol;
        public string address;
    }
}
