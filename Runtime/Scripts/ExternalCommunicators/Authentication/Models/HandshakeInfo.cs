#nullable enable
namespace ElympicsPlayPad.ExternalCommunicators.Authentication.Models
{
    public readonly struct HandshakeInfo
    {
        public readonly bool IsMobile;
        public readonly Capabilities Capabilities;
        public readonly FeatureAccess FeatureAccess;
        public readonly string Environment;
        public readonly string ClosestRegion;
        public readonly UserPrefsInfo UserPrefs;
        public readonly LaunchMode LaunchMode;

        public HandshakeInfo(
            bool isMobile,
            Capabilities capabilities,
            string environment,
            string closestRegion,
            FeatureAccess featureAccess,
            UserPrefsInfo userPrefs,
            LaunchMode launchMode)
        {
            IsMobile = isMobile;
            Capabilities = capabilities;
            Environment = environment;
            ClosestRegion = closestRegion;
            FeatureAccess = featureAccess;
            UserPrefs = userPrefs;
            LaunchMode = launchMode;

        }
    }

    public readonly struct UserPrefsInfo
    {
        public readonly string[] Languages;

        public UserPrefsInfo(string[] languages) => Languages = languages;
    }
}
