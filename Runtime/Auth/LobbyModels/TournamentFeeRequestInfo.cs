#nullable enable

namespace ElympicsPlayPad.Lobby
{
    public readonly struct TournamentFeeRequestInfo
    {
        public CoinInfo CoinInfo { get; init; }
        public int PlayersCount { get; init; }
        public decimal Prize { get; init; }
        public decimal[]? PrizeDistribution { get; init; }
    }
}
