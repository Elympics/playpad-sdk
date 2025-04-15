using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;
namespace ElympicsPlayPad.ExternalCommunicators.Authentication.Extensions
{
    public static class FeatureAccessExt
    {
        public static bool HasNone(this FeatureAccess featureAccess) => featureAccess == FeatureAccess.None;

        public static bool HasAuthentication(this FeatureAccess featureAccess) => featureAccess.HasFlag(FeatureAccess.Authentication);
        public static bool HasLeaderboard(this FeatureAccess featureAccess) => featureAccess.HasFlag(FeatureAccess.Leaderboard);
        public static bool HasTournament(this FeatureAccess featureAccess) => featureAccess.HasFlag(FeatureAccess.Tournament);
        public static bool HasUserHighScore(this FeatureAccess featureAccess) => featureAccess.HasFlag(FeatureAccess.HighScore);

        public static bool HasVirtualDeposit(this FeatureAccess featureAccess) => featureAccess.HasFlag(FeatureAccess.VirtualDeposit);
        public static bool HasOnlyAuthentication(this FeatureAccess featureAccess) => featureAccess == FeatureAccess.Authentication;
        public static bool HasOnlyLeaderboard(this FeatureAccess featureAccess) => featureAccess == FeatureAccess.Leaderboard;
        public static bool HasOnlyTournament(this FeatureAccess featureAccess) => featureAccess == FeatureAccess.Tournament;
        public static bool HasOnlyUserHighScore(this FeatureAccess featureAccess) => featureAccess == FeatureAccess.HighScore;
        public static bool HasOnlyVirtualDepositScore(this FeatureAccess featureAccess) => featureAccess == FeatureAccess.VirtualDeposit;
    }
}
