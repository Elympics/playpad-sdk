using Elympics;
using UnityEngine;
namespace ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js
{
    internal static class PlayPadExtensionAuthentication
    {
        internal const string AUTHORIZED_GAMEID_KEY = "PlayPadAuthorizationGameId";
        internal const string AUTHORIZED_GAMEVERSIONID_KEY = "PlayPadAuthorizationGameVersionId";
        internal const string AUTHORIZED_DEVELOPER_JWT_KEY = "PlayPadAuthorizationDeveloperToken";
        internal const string EXPIRATION_KEY = "PlayPadAuthorizationExpiration";

        public static bool IsAuthenticated(string gameId, string gameVersionId, string developerJwt)
        {
            var authorizationStatus = GetAuthorizationStatus();
            if (authorizationStatus == null)
                return false;
            if (gameId != authorizationStatus.Value.GameId || gameVersionId != authorizationStatus.Value.GameVersion || developerJwt != authorizationStatus.Value.DeveloperToken)
                return false;
            return !IsExpired(authorizationStatus.Value.ExpirationDate);
        }


        public static void Authenticate(string gameId, string gameVersionId, string developerJwt)
        {
            PlayerPrefs.SetString(AUTHORIZED_GAMEID_KEY, gameId);
            PlayerPrefs.SetString(AUTHORIZED_GAMEVERSIONID_KEY, gameVersionId);
            PlayerPrefs.SetString(AUTHORIZED_DEVELOPER_JWT_KEY, developerJwt);
            PlayerPrefs.SetString(EXPIRATION_KEY, TimeUtil.DateTimeNowAsString);
        }

        private static AuthorizationStatus? GetAuthorizationStatus()
        {
            var gameId = PlayerPrefs.GetString(AUTHORIZED_GAMEID_KEY, "");
            if (string.IsNullOrEmpty(gameId))
                return null;
            var gameVersionId = PlayerPrefs.GetString(AUTHORIZED_GAMEVERSIONID_KEY, "");
            if (string.IsNullOrEmpty(gameVersionId))
                return null;
            var developerToken = PlayerPrefs.GetString(AUTHORIZED_DEVELOPER_JWT_KEY, "");
            if (string.IsNullOrEmpty(developerToken))
                return null;
            var expirationDate = PlayerPrefs.GetString(EXPIRATION_KEY, "");
            if (string.IsNullOrEmpty(expirationDate))
                return null;

            return new AuthorizationStatus
            {
                GameId = gameId,
                GameVersion = gameVersionId,
                DeveloperToken = developerToken,
                ExpirationDate = expirationDate
            };
        }

        private static bool IsExpired(string _)
        {
            return true; // as for today, we do not have implement any extension jwt generator at all. So all tokens are treated as expired.
            // Extension developer has to implement this and pass it via ExtensionHandshake
            // kpieta 28.11.2025
            //var expirationDate = TimeUtil.DateTimeFromString(date);
            //return expirationDate < DateTime.UtcNow;
        }
    }


    internal readonly struct AuthorizationStatus
    {
        public readonly string GameId { get; init; }
        public readonly string GameVersion { get; init; }
        public readonly string DeveloperToken { get; init; }
        public readonly string ExpirationDate { get; init; }
    }
}
