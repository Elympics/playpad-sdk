#nullable enable
using JetBrains.Annotations;
namespace ElympicsPlayPad.ExternalCommunicators.VirtualDeposit.Models
{
    [PublicAPI]
    public readonly struct WalletBalanceInfo
    {
        public string AmountRaw { get; init; }
        public decimal Amount { get; init; }
        public string? Error { get; init; }
    }
}
