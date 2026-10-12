#nullable enable

using System;
using System.Linq;
using Elympics.Core.Utils;
using JetBrains.Annotations;

namespace ElympicsPlayPad.Rooms.Models
{
    public class CompetitivenessConfig
    {
        /// <summary>Determines how the other fields are interpreted.</summary>
        internal readonly CompetitivenessType CompetitivenessType;
        /// <summary>Tournament ID, rolling tournament type ID or coin ID.</summary>
        internal readonly string ID;
        /// <summary>Bet amount, or the total prize of a rolling tournament.</summary>
        internal readonly decimal Value;
        /// <summary>Only used with <see cref="CompetitivenessType.RollingTournament"/>.</summary>
        internal readonly int NumberOfPlayers;
        /// <summary>Only used with <see cref="CompetitivenessType.RollingTournament"/>.</summary>
        internal readonly decimal[] PrizeDistribution;

        [PublicAPI]
        public static CompetitivenessConfig GlobalTournament(string id) => new(id, CompetitivenessType.GlobalTournament, 0, -1, Array.Empty<decimal>());

        /// <param name="numberOfPlayers">Players who must play a match in the tournament for it to finish.</param>
        /// <param name="prizeDistribution">
        /// Share of <paramref name="totalPrize"/> for each leaderboard place, summing up to ~1. Missing places get 0.
        /// Null or empty gives the whole prize to the top player.
        /// </param>
        [PublicAPI]
        public static CompetitivenessConfig RollingTournament(int numberOfPlayers, decimal totalPrize, Guid coinId, decimal[]? prizeDistribution = null)
        {
            const decimal tolerance = 0.01m;

            if (numberOfPlayers <= 1)
                throw new ArgumentOutOfRangeException(nameof(numberOfPlayers), numberOfPlayers, "The number of players must be greater than one.");
            if (prizeDistribution != null && numberOfPlayers < prizeDistribution.Length)
                throw new ArgumentException($"{nameof(prizeDistribution)} can't include more places on the leaderboard than there are players in the tournament. {nameof(numberOfPlayers)}: {numberOfPlayers} {nameof(prizeDistribution)}.{nameof(prizeDistribution.Length)}: {prizeDistribution.Length}.", nameof(prizeDistribution));
            if (prizeDistribution != null && Math.Abs(prizeDistribution.Sum() - 1m) > tolerance)
                throw new ArgumentException($"{nameof(prizeDistribution)} values have to sum up to 1 +/-{tolerance}. Sum: {prizeDistribution.Sum()} Values: {prizeDistribution.CommaList()}.", nameof(prizeDistribution));

            return new CompetitivenessConfig(coinId.ToString(), CompetitivenessType.RollingTournament, totalPrize, numberOfPlayers, prizeDistribution ?? Array.Empty<decimal>());
        }

        /// <param name="amount">The entry fee.</param>
        /// <remarks>Each player on the winning team receives <paramref name="amount"/> minus commission.</remarks>
        [PublicAPI]
        public static CompetitivenessConfig Bet(Guid coinId, decimal amount) => new(coinId.ToString(), CompetitivenessType.Bet, amount, -1, Array.Empty<decimal>());

        private CompetitivenessConfig(string tournamentId, CompetitivenessType competitivenessType, decimal value, int numberOfPlayers, decimal[] prizeDistribution)
        {
            ID = tournamentId ?? throw new ArgumentNullException(nameof(tournamentId));
            CompetitivenessType = competitivenessType;
            Value = value;
            NumberOfPlayers = numberOfPlayers;
            PrizeDistribution = prizeDistribution;
        }
    }
}
