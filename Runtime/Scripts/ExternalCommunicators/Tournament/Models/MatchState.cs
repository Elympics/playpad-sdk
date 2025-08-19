namespace ElympicsPlayPad.ExternalCommunicators.Tournament.Models
{
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
        Failed,
        /// <summary>
        /// Unexpected state was received from PlayPad. Try updating PlayPad SDK to resolve this issue.
        /// </summary>
        Unknown
    }
}
