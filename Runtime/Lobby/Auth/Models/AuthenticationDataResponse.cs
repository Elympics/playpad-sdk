using System;
using Elympics.Models.Authentication;

namespace ElympicsPlayPad.Auth.Models
{
    [Serializable]
    internal class AuthenticationDataResponse
    {
        public string userId;
        public string jwtToken;
        public string nickname;

        public AuthData ToAuthData(AuthType authType) => new(new Guid(userId), jwtToken, authType);
    }
}
