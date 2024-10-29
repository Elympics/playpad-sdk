using System;

namespace ElympicsPlayPad.DTO
{
    [Serializable]
    public class TransactionToSign
    {
        public string from;
        public string to;
        public string data;
    }
}
