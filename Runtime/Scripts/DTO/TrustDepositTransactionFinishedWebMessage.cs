using System;

namespace ElympicsPlayPad.DTO
{
    [Serializable]
    internal class TrustDepositTransactionFinishedWebMessage
    {
        public int status;
        public string errorMessage;
        public float increasedAmount;
        public CheckDepositResponse trustState;
    }
}
