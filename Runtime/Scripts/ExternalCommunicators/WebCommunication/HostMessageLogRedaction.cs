#nullable enable
using System;
using System.Text;

namespace ElympicsPlayPad.ExternalCommunicators.WebCommunication
{
    /// <summary>
    /// Masks the values of credential keys in host-page message JSON before it is logged.
    /// Handles plain JSON and JSON nested in a string (escaped quotes), as in <c>response</c> and <c>message</c> fields.
    /// </summary>
    internal static class HostMessageLogRedaction
    {
        internal const string Mask = "***";
        private static readonly string[] SensitiveKeys = { "jwt", "jwtToken", "userSecret", "clientSecret", "joinToken", "credential" };

        public static string Redact(string? json)
        {
            if (string.IsNullOrEmpty(json))
                return json ?? string.Empty;

            StringBuilder? sb = null;
            var copiedUpTo = 0;
            var position = 0;
            while (TryFindNextSecretValue(json, position, out var valueStart, out var valueEnd))
            {
                sb ??= new StringBuilder(json.Length);
                _ = sb.Append(json, copiedUpTo, valueStart - copiedUpTo).Append(Mask);
                copiedUpTo = valueEnd;
                position = valueEnd;
            }

            if (sb == null)
                return json;
            _ = sb.Append(json, copiedUpTo, json.Length - copiedUpTo);
            return sb.ToString();
        }

        private static bool TryFindNextSecretValue(string json, int startIndex, out int valueStart, out int valueEnd)
        {
            valueStart = valueEnd = -1;
            foreach (var key in SensitiveKeys)
            {
                var searchFrom = startIndex;
                while (true)
                {
                    var keyIndex = json.IndexOf(key, searchFrom, StringComparison.Ordinal);
                    if (keyIndex < 0)
                        break;
                    searchFrom = keyIndex + key.Length;
                    if (!TryGetStringValueStart(json, keyIndex, key.Length, out var start))
                        continue;
                    if (valueStart < 0 || start < valueStart)
                    {
                        valueStart = start;
                        valueEnd = FindValueEnd(json, start);
                    }
                    break;
                }
            }
            return valueStart >= 0;
        }

        // Accepts "key": "value" and \"key\": \"value\" (whitespace allowed around the colon).
        private static bool TryGetStringValueStart(string json, int keyIndex, int keyLength, out int valueStart)
        {
            valueStart = -1;
            if (keyIndex == 0 || json[keyIndex - 1] != '"')
                return false;
            var escaped = keyIndex >= 2 && json[keyIndex - 2] == '\\';

            var i = keyIndex + keyLength;
            if (!TrySkipQuote(json, ref i, escaped))
                return false;
            SkipWhitespace(json, ref i);
            if (i >= json.Length || json[i] != ':')
                return false;
            i++;
            SkipWhitespace(json, ref i);
            if (!TrySkipQuote(json, ref i, escaped))
                return false;
            valueStart = i;
            return true;
        }

        private static bool TrySkipQuote(string json, ref int i, bool escaped)
        {
            if (escaped)
            {
                if (i + 1 >= json.Length || json[i] != '\\' || json[i + 1] != '"')
                    return false;
                i += 2;
                return true;
            }
            if (i >= json.Length || json[i] != '"')
                return false;
            i++;
            return true;
        }

        private static void SkipWhitespace(string json, ref int i)
        {
            while (i < json.Length && char.IsWhiteSpace(json[i]))
                i++;
        }

        // Credential values (JWTs, GUIDs, base64) contain neither quotes nor backslashes.
        private static int FindValueEnd(string json, int valueStart)
        {
            var index = valueStart;
            while (index < json.Length && json[index] is not ('"' or '\\'))
                index++;
            return index;
        }
    }
}
