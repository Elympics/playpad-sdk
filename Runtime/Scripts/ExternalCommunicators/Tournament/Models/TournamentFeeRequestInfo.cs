using ElympicsPlayPad.ExternalCommunicators.VirtualDeposit.Models;
namespace ElympicsPlayPad.Tournament.Data
{
    public readonly struct TournamentFeeRequestInfo
    {
        public CoinInfo CoinInfo { get; init; }
        public int PlayersCount { get; init; }
        public decimal Prize { get; init; }
    }
}
