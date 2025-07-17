#nullable enable
using Elympics;
using JetBrains.Annotations;

namespace ElympicsPlayPad.Tournament.Data
{
    [PublicAPI]
    public readonly struct RollingTournamentPrizeDetails
    {
        public readonly decimal Prize;
        public readonly CoinInfo Coin;
        public readonly decimal EntryFee;

        public RollingTournamentPrizeDetails(decimal prize, CoinInfo coin, decimal entryFee)
        {
            Prize = prize;
            Coin = coin;
            EntryFee = entryFee;
        }
    }
}
