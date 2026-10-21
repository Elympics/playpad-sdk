#nullable enable
using System;
using Elympics;
using UnityEngine;

namespace ElympicsPlayPad.Wrappers
{
    /// <summary>The guest login shared by <see cref="DefaultElympicsLobbyWrapper"/> and the standalone authenticator, so both log in as the same user.</summary>
    internal static class ClientSecretAuth
    {
        private const string ClientSecretPlayerPrefsKeyBase = "Elympics/AuthToken";

        private static string? clientSecret;

        private static string ClientSecretPlayerPrefsKey =>
            PlayModeInstance.PlayerName is { } playerName ? $"{ClientSecretPlayerPrefsKeyBase}_{playerName}" : ClientSecretPlayerPrefsKeyBase;

        internal static string GetAuthUrl(ElympicsConfig config) =>
            ApplicationParameters.Parameters.AuthEndpoint.GetValue(config.GetV2Endpoint("auth")).GetAbsoluteOrRelativeString();

        /// <remarks>Cached for the session, so an empty client secret parameter creates one new user, not one per caller.</remarks>
        internal static string GetOrCreateClientSecret() => clientSecret ??= ReadOrCreateClientSecret();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache() => clientSecret = null;

        private static string ReadOrCreateClientSecret()
        {
            var parameterValue = ApplicationParameters.Parameters.ClientSecret.GetValue();
            if (!string.IsNullOrEmpty(parameterValue))
                return parameterValue!;

            if (parameterValue == "")
                return CreateNewClientSecret();

            var key = ClientSecretPlayerPrefsKey;
            if (!PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.SetString(key, CreateNewClientSecret());
                PlayerPrefs.Save();
            }
            return PlayerPrefs.GetString(key);
        }

        private static string CreateNewClientSecret() => Guid.NewGuid().ToString();
    }
}
