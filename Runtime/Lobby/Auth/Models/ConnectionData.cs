using Elympics.Models.Authentication;
using JetBrains.Annotations;

namespace ElympicsPlayPad.Auth.Models
{
    [PublicAPI]
    public struct ConnectionData
    {
        /// <summary>
        /// Type of authentication to be performed
        /// </summary>
        public AuthType? AuthType;

        /// <summary>
        /// If not provided, the current region is used.
        /// If it's only data provided, Elympics will set new region and auto-rejoin lobby.
        /// </summary>
        public RegionData? Region;

        /// <summary>
        /// If not null, Elympics will try to connect using cached data.
        /// </summary>
        public CachedAuthData? AuthFromCacheData;
    }
}
