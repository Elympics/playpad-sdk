#nullable enable
using System;

namespace ElympicsPlayPad.ExternalCommunicators.Authentication.Models
{
    internal static class GameModes
    {
        internal const string Standard = "standard";

        /// <returns>The trimmed mode, or <c>null</c> for the standard flow (missing, blank or "standard").</returns>
        internal static string? Normalize(string? gameMode)
        {
            var trimmed = gameMode?.Trim();
            return string.IsNullOrEmpty(trimmed) || string.Equals(trimmed, Standard, StringComparison.Ordinal)
                ? null
                : trimmed;
        }
    }
}
