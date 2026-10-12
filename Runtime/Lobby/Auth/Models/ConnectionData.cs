using Elympics.Models.Authentication;
using JetBrains.Annotations;

namespace ElympicsPlayPad.Auth.Models
{
    [PublicAPI]
    public struct ConnectionData
    {
        public AuthType? AuthType;

        /// <summary>Null means the current region. Given alone, it switches the region and rejoins the lobby.</summary>
        public RegionData? Region;

        public CachedAuthData? AuthFromCacheData;
    }
}
