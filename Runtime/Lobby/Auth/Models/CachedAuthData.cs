using Elympics.Models.Authentication;
using JetBrains.Annotations;

namespace ElympicsPlayPad.Auth.Models
{
    [PublicAPI]
    public struct CachedAuthData
    {
        public CachedAuthData(AuthData cachedData, bool autoRetryIfExpired)
        {
            CachedData = cachedData;
            AutoRetryIfExpired = autoRetryIfExpired;
        }

        public AuthData CachedData { get; init; }

        /// <summary>If the JWT has expired, authenticate again with the cached <see cref="AuthData.AuthType"/>.</summary>
        public bool AutoRetryIfExpired { get; init; }
    }
}
