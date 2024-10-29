using System;

namespace ElympicsPlayPad.DTO
{
    [Serializable]
    public class WalletConnectionWebMessage
    {
        public bool isConnected;
        public string address;
        public string chainId;
    }
}

