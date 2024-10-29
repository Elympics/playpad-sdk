
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;
namespace ElympicsPlayPad.ExternalCommunicators.Authentication.Extensions
{
    public static class FeatureAccessExt
    {
        public static bool HasNone(this FeatureAccess capabilities) => capabilities == FeatureAccess.None;

        public static bool HasAuthentication(this FeatureAccess capabilities) => capabilities.HasFlag(FeatureAccess.Authentication);
        public static bool HasLeaderboard(this FeatureAccess capabilities) => capabilities.HasFlag(FeatureAccess.Leaderboard);
        public static bool HasTournament(this FeatureAccess capabilities) => capabilities.HasFlag(FeatureAccess.Tournament);
        public static bool HasUserHighScore(this FeatureAccess capabilities) => capabilities.HasFlag(FeatureAccess.HighScore);

        public static bool HasOnlyAuthentication(this FeatureAccess capabilities) => capabilities == FeatureAccess.Authentication;
        public static bool HasOnlyLeaderboard(this FeatureAccess capabilities) => capabilities == FeatureAccess.Leaderboard;
        public static bool HasOnlyTournament(this FeatureAccess capabilities) => capabilities == FeatureAccess.Tournament;
        public static bool HasOnlyUserHighScore(this FeatureAccess capabilities) => capabilities == FeatureAccess.HighScore;
    }
}
