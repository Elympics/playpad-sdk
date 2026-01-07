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
        public readonly LaunchMode LaunchMode;

        public HandshakeInfo(
            bool isMobile,
            Capabilities capabilities,
            string environment,
            string closestRegion,
            FeatureAccess featureAccess,
            LaunchMode launchMode)
        {
            IsMobile = isMobile;
            Capabilities = capabilities;
            Environment = environment;
            ClosestRegion = closestRegion;
            FeatureAccess = featureAccess;
            LaunchMode = launchMode;
        }
    }
}
