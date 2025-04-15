using System;
using System.Numerics;
namespace ElympicsPlayPad.ExternalCommunicators.VirtualDeposit.Models
{
    public readonly struct VirtualDepositInfo
    {
        public decimal Amount { get; init; }
        public string Wei { get; init; }
        public CoinInfo CoinInfo { get; init; }
    }
}
