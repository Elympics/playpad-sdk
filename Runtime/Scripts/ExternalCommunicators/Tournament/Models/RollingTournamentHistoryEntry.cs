#nullable enable

using System;
using System.Collections.ObjectModel;
using Elympics;
using ElympicsPlayPad.ExternalCommunicators.Tournament.Models;
using JetBrains.Annotations;

namespace ElympicsPlayPad.Tournament.Data
{
    [PublicAPI]
    public readonly struct RollingTournamentHistory
    {
        public readonly RollingTournamentHistoryEntry[] Entries;

        public RollingTournamentHistory(RollingTournamentHistoryEntry[] entries) => Entries = entries;
    }

    [PublicAPI]
    public readonly struct RollingTournamentHistoryEntry
    {
        [Obsolete("Use CurrentState instead.")]
        public string State => CurrentState.ToString();

        public readonly TournamentState CurrentState;

        /// <remarks>Can be null if coin used in this tournament is currently not available due to updated game configuration or platform on which the game client is currently launched.</remarks>
        public readonly RollingTournamentPrizeDetails? PrizeDetails;

        public readonly int NumberOfPlayers;

        /// <summary>All matches played in this tournament so far in the order of places on the leaderboard.</summary>
        public readonly ReadOnlyCollection<RollingTournamentMatch> AllMatches;

        public readonly bool NewSettlement;

#pragma warning disable CS0618 // Type or member is obsolete. LocalPlayerMatchIndex will be made private in the future.
        public RollingTournamentMatch LocalPlayerMatch => AllMatches[LocalPlayerMatchIndex];
#pragma warning restore CS0618

        /// <remarks>Can be null if coin used in this tournament is currently not available due to updated game configuration or platform on which the game client is currently launched.</remarks>
        [Obsolete("Use " + nameof(PrizeDetails) + " instead.")]
        public decimal? Prize => PrizeDetails?.Prize;

        /// <remarks>Can be null if coin used in this tournament is currently not available due to updated game configuration or platform on which the game client is currently launched.</remarks>
        [Obsolete("Use " + nameof(PrizeDetails) + " instead.")]
        public CoinInfo? Coin => PrizeDetails?.Coin;

        /// <remarks>Can be null if coin used in this tournament is currently not available due to updated game configuration or platform on which the game client is currently launched.</remarks>
        [Obsolete("Use " + nameof(PrizeDetails) + " instead.")]
        public decimal? EntryFee => PrizeDetails?.EntryFee;

        /// <summary>Index of the local player's match in <see cref="AllMatches"/>.</summary>
        [Obsolete("Use " + nameof(LocalPlayerMatch) + " instead.")]
        public readonly int LocalPlayerMatchIndex;

        public RollingTournamentHistoryEntry(
            TournamentState state,
            RollingTournamentPrizeDetails? prizeDetails,
            int numberOfPlayers,
            ReadOnlyCollection<RollingTournamentMatch> allMatches,
            int localPlayerMatchIndex,
            bool newSettlement)
        {
            if (localPlayerMatchIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(localPlayerMatchIndex));
            if (localPlayerMatchIndex >= allMatches.Count)
                throw new ArgumentException(
                    $"{nameof(localPlayerMatchIndex)} is not a valid index for {nameof(allMatches)}. {nameof(localPlayerMatchIndex)}: {localPlayerMatchIndex} {nameof(allMatches)}.Count: {allMatches.Count}.",
                    nameof(localPlayerMatchIndex));

            CurrentState = state;
            PrizeDetails = prizeDetails;
            NumberOfPlayers = numberOfPlayers;
            AllMatches = allMatches;
#pragma warning disable CS0618 // Type or member is obsolete. This member will be made private in the future.
            LocalPlayerMatchIndex = localPlayerMatchIndex;
#pragma warning restore CS0618
            NewSettlement = newSettlement;
        }

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
            /// Unexpected state was received from PlayPad. Try updating PlayPad SDK to resolve this issue.
            /// </summary>
            Unknown
        }
    }

    [PublicAPI]
    public readonly struct RollingTournamentMatch : IEquatable<RollingTournamentMatch>
    {
        public readonly MatchState State;
        public readonly string AvatarUrl;
        public readonly string Nickname;
        public readonly DateTime MatchEnded;
        public readonly uint Position;
        public readonly decimal Prize;
        public readonly float Score;

        public RollingTournamentMatch(
            string avatarUrl,
            string nickname,
            DateTime matchEnded,
            float score,
            MatchState state,
            decimal prize,
            uint position)
        {
            AvatarUrl = avatarUrl;
            Nickname = nickname;
            MatchEnded = matchEnded;
            Score = score;
            State = state;
            Prize = prize;
            Position = position;
        }

        public bool Equals(RollingTournamentMatch other) => AvatarUrl == other.AvatarUrl && Nickname == other.Nickname && MatchEnded.Equals(other.MatchEnded) && Score.Equals(other.Score) && Position.Equals(other.Position) && Prize.Equals(other.Prize) && State == other.State;

        public override bool Equals(object? obj) => obj is RollingTournamentMatch other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(AvatarUrl, Nickname, MatchEnded, Score);

        public static bool operator ==(RollingTournamentMatch left, RollingTournamentMatch right) => left.Equals(right);

        public static bool operator !=(RollingTournamentMatch left, RollingTournamentMatch right) => !left.Equals(right);
    }
}
