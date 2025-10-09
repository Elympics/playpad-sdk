#nullable enable
using System;
using Elympics;
using JetBrains.Annotations;

namespace ElympicsPlayPad.Tournament.Data
{
    [PublicAPI]
    public readonly struct RollingTournamentPrizeDetails
    {
        [Obsolete("Will return prize for first place. Use " + nameof(Prizes) + " for prize distribution.")]
        public decimal Prize => Prizes[0];

        public readonly decimal[] Prizes;
        public readonly CoinInfo Coin;
        public readonly decimal EntryFee;

        public RollingTournamentPrizeDetails(CoinInfo coin, decimal entryFee, decimal[] prizes)
        {
            Coin = coin;
            EntryFee = entryFee;
            Prizes = prizes;
        }
    }
}
