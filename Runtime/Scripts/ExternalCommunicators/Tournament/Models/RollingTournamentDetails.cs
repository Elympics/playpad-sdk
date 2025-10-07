#nullable enable

using System;
using System.Collections.ObjectModel;
using Elympics.Communication.Authentication.Models;
using ElympicsPlayPad.ExternalCommunicators.Tournament.Models;
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
        /// <remarks>If this property is not null, this match is also included in <see cref="AllMatches"/>.</remarks>
        public RollingTournamentMatchDetails? LocalPlayerMatch => _localPlayerMatchIndex > -1 ? AllMatches[_localPlayerMatchIndex] : null;

        /// <summary>Index of the local player's match in <see cref="AllMatches"/> or -1 if <see cref="State"/> is <see cref="TournamentState.YourResultsPending"/>.</summary>
        private readonly int _localPlayerMatchIndex;

        public enum TournamentState
        {
            /// <summary>Tournament is live and local player's result is included in <see cref="AllMatches"/>.</summary>
            Live,
            /// <summary>
            /// Tournament was finished because the expected number of players joined and finished their matches or
            /// the matchmaking system was unable to find enough players to complete the tournament in reasonable time,
            /// so the tournament was cancelled.
            /// </summary>
            Finished,
            /// <summary>
            /// Same as <see cref="Live"/>, but the local player participated in the tournament recently and their results
            /// are still being processed, so they are not included in <see cref="AllMatches"/> yet.
            /// </summary>
            YourResultsPending,
            /// <summary>
            /// The tournament was cancelled, because the matchmaking system was unable to find enough players in 24h since the tournament's creation.
            /// </summary>
            Cancelled,
            /// <summary>
            /// Unexpected state was received from PlayPad. Try updating PlayPad SDK to resolve this issue.
            /// </summary>
            Unknown,
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
        /// <summary>Date and time when the match was finished. Null when <see cref="State"/> is <see cref="MatchState.Playing"/>.</summary>
        public readonly DateTime? MatchEnded;
        /// <summary>Final score of the match if <see cref="State"/> is <see cref="MatchState.Finished"/>, otherwise 0.</summary>
        public readonly float Score;
        /// <summary>Current position on leaderboard. Null when <see cref="State"/> is <see cref="MatchState.Playing"/>.</summary>
        public readonly uint? Position;
        /// <summary>The user who played in the match.</summary>
        public readonly ElympicsUser User;

        [Obsolete("Use " + nameof(User) + "." + nameof(ElympicsUser.AvatarUrl) + " instead.")]
        public string AvatarUrl => User.AvatarUrl;
        [Obsolete("Use " + nameof(User) + "." + nameof(ElympicsUser.Nickname) + " instead.")]
        public string Nickname => User.Nickname;

        public RollingTournamentMatchDetails(MatchState state, DateTime? matchEnded, float score, uint? position, ElympicsUser user)
        {
            State = state;
            MatchEnded = matchEnded;
            Score = score;
            Position = position;
            User = user;
        }
    }
}
