#nullable enable

using System;
using System.Collections.ObjectModel;
using Elympics;
using JetBrains.Annotations;

namespace ElympicsPlayPad.Tournament.Data
{
    [PublicAPI]
    public readonly struct RollingTournamentDetails
    {
        public readonly TournamentState State;
        /// <summary>The number of players that have to play a match in this tournament in order for the tournament to be finished.</summary>
        public readonly int NumberOfPlayers;
        /// <summary>
        /// All matches played in this tournament so far in the order of places on the leaderboard.
        /// Unfinished matches (ones that don't have a score yet, so they can't be assigned a place) appear in this collection after all finished matches.
        /// </summary>
        public readonly ReadOnlyCollection<RollingTournamentMatchDetails> AllMatches;
        /// <remarks>Can be null if coin used in this tournament is currently not available due to updated game configuration or platform on which the game client is currently launched.</remarks>
        public readonly RollingTournamentPrizeDetails? PrizeDetails;
        /// <summary>Local player's match or null if <see cref="State"/> is <see cref="TournamentState.YourResultsPending"/>.</summary>
        /// <remarks>If this propert is not null, this match is also included in <see cref="AllMatches"/>.</remarks>
        public RollingTournamentMatchDetails? LocalPlayerMatch => _localPlayerMatchIndex > -1 ? AllMatches[_localPlayerMatchIndex] : null;

        /// <summary>Index of the local player's match in <see cref="AllMatches"/> or -1 if <see cref="State"/> is <see cref="TournamentState.YourResultsPending"/>.</summary>
        private readonly int _localPlayerMatchIndex;

        public enum TournamentState
        {
            /// <summary>Tournament is live and if the local player participated in it, their result is included in <see cref="AllMatches"/>.</summary>
            Live,
            /// <summary>Tournament was finished because the expected number of players joined and finished their matches or a timeout occured.</summary>
            Finished,
            /// <summary>
            /// Same as <see cref="Live"/>, but the local player participated in the tournament recently and their results
            /// are still being processed, so they are not included in <see cref="AllMatches"/> yet.
            /// </summary>
            YourResultsPending
        }

        public RollingTournamentDetails(TournamentState state, RollingTournamentPrizeDetails? prizeDetails, int numberOfPlayers, ReadOnlyCollection<RollingTournamentMatchDetails> allMatches, int localPlayerMatchIndex)
        {
            State = state;
            PrizeDetails = prizeDetails;
            NumberOfPlayers = numberOfPlayers;
            AllMatches = allMatches;
            _localPlayerMatchIndex = localPlayerMatchIndex;
        }
    }

    [PublicAPI]
    public readonly struct RollingTournamentMatchDetails
    {
        public readonly MatchState State;
        public readonly string AvatarUrl;
        public readonly string Nickname;
        /// <summary>Date and time when a match was finished. Null when <see cref="State"/> is <see cref="MatchState.Playing"/>.</summary>
        public readonly DateTime? MatchEnded;
        /// <summary>Final score of a match if <see cref="State"/> is <see cref="MatchState.Finished"/>, otherwise 0.</summary>
        public readonly float Score;
        /// <summary>Current position on leaderboard. Null when <see cref="State"/> is <see cref="MatchState.Playing"/>.</summary>
        public readonly uint? Position;

        public enum MatchState
        {
            /// <summary>Match is currently being played.</summary>
            Playing,
            /// <summary>Match was successfully finished and is included in the tournament leaderboard.</summary>
            Finished,
            /// <summary>
            /// Match was started, but failed to finish. This can happen when a player disconnects from a match before it ends.
            /// A failed match counts towards the total number of matches in a tournament, but has no score.
            /// If all matches in a tournament end with a failure the tournament ends with a tie and all players receive equal rewards from the reward pool.
            /// </summary>
            Failed
        }

        public RollingTournamentMatchDetails(MatchState state, string avatarUrl, string nickname, DateTime? matchEnded, float score, uint? position)
        {
            State = state;
            AvatarUrl = avatarUrl;
            Nickname = nickname;
            MatchEnded = matchEnded;
            Score = score;
            Position = position;
        }
    }

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
