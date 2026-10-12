using System;
using System.Text;
using ElympicsPlayPad.Auth.Models;
using UnityEngine;

namespace ElympicsPlayPad.Auth
{
    internal static class JwtTokenUtil
    {
        public static bool IsJwtExpired(string cachedDataJwtToken)
        {
            var split = cachedDataJwtToken.Split('.');
            if (split.Length != 3)
                throw new ArgumentException("Token must consist from 3 delimited by dot parts");

            var payloadData = Base64Url.Decode(split[1]);
            var data = Encoding.UTF8.GetString(payloadData);
            var payload = JsonUtility.FromJson<JwtTokenPayload>(data);
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() > payload.exp;
        }
    }
}


