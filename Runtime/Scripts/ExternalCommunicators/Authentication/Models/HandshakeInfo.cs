#nullable enable
using Elympics.Models.Authentication;
using ElympicsPlayPad.Tournament.Data;

namespace ElympicsPlayPad.ExternalCommunicators.Authentication.Models
{
    public readonly struct HandshakeInfo
    {
        public readonly bool IsMobile;
        public readonly Capabilities Capabilities;
        public readonly FeatureAccess FeatureAccess;
        public readonly string Environment;
        public readonly string ClosestRegion;

        public HandshakeInfo(bool isMobile, Capabilities capabilities, string environment, string closestRegion, FeatureAccess featureAccess)
        {
            IsMobile = isMobile;
            Capabilities = capabilities;
            Environment = environment;
            ClosestRegion = closestRegion;
            FeatureAccess = featureAccess;
        }
    }
}
