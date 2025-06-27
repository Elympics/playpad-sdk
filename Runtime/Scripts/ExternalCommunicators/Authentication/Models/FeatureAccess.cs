using System;

namespace ElympicsPlayPad.ExternalCommunicators.Authentication.Models
{
    [Flags]
    public enum FeatureAccess
    {
        None = 0,
        Authentication = 1,
        Leaderboard = 2,
        Tournament = 4,
        HighScore = 8,
        VirtualDeposit = 16,
    }
}
