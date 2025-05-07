using System;

namespace ElympicsPlayPad.Leaderboard
{
    public readonly struct UserHighScoreInfo
    {
        public float Points { get; init; }
        public DateTime? ScoredAt { get; init; }
    }
}
