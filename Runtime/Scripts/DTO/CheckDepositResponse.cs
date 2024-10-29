using System;

namespace ElympicsPlayPad.DTO
{
    [Serializable]
    internal class CheckDepositResponse
    {
        public float totalAmount;
        public float lockedPendingSettlement;
        public float lockedPendingWithdrawal;
        public float Available => totalAmount - lockedPendingSettlement - lockedPendingWithdrawal;
    }
}
