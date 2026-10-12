using System;

namespace ElympicsPlayPad.Auth
{
    internal static class Base64Url
    {
        public static byte[] Decode(string input)
        {
            var output = input.Replace('-', '+').Replace('_', '/');
            switch (output.Length % 4)
            {
                case 0:
                    break;
                case 2:
                    output += "==";
                    break;
                case 3:
                    output += "=";
                    break;
                default:
                    throw new Exception("Illegal base64url string!");
            }
            return Convert.FromBase64String(output);
        }
    }
}
