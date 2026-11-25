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

        /// <summary>
        /// The game mode the host asked for. <c>null</c> means the standard online flow; any other value is an offline mode (see <c>offline-game-modes.md</c>).
        /// </summary>
        public readonly string? GameMode;

        public HandshakeInfo(
            bool isMobile,
            Capabilities capabilities,
            string environment,
            string closestRegion,
            FeatureAccess featureAccess,
            UserPrefsInfo userPrefs,
            LaunchMode launchMode)
            : this(isMobile, capabilities, environment, closestRegion, featureAccess, userPrefs, launchMode, null)
        {
        }

        /// <param name="gameMode">The game mode, already normalised: <c>null</c> for the standard online flow, otherwise an offline mode.</param>
        public HandshakeInfo(
            bool isMobile,
            Capabilities capabilities,
            string environment,
            string closestRegion,
            FeatureAccess featureAccess,
            UserPrefsInfo userPrefs,
            LaunchMode launchMode,
            string? gameMode)
        {
            IsMobile = isMobile;
            Capabilities = capabilities;
            Environment = environment;
            ClosestRegion = closestRegion;
            FeatureAccess = featureAccess;
            UserPrefs = userPrefs;
            LaunchMode = launchMode;
            GameMode = gameMode;
        }
    }

    public readonly struct UserPrefsInfo
    {
        public readonly string[] Languages;

        public UserPrefsInfo(string[] languages) => Languages = languages;
    }
}
