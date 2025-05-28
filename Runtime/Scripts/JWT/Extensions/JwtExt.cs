using System;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Utility;
using UnityEngine;

namespace ElympicsPlayPad.JWT.Extensions
{
    public static class JwtExt
    {
        public static JwtPayload ExtractUnityPayloadFromJwt(this string jwt)
        {
            var payload = JsonWebToken.Decode(jwt, string.Empty, false) ?? throw new Exception("Couldn't decode payload form jwt token.");
            var formattedPayload = AuthTypeRawUtility.ToUnityNaming(payload);
            return JsonUtility.FromJson<JwtPayload>(formattedPayload);
        }
    }
}
