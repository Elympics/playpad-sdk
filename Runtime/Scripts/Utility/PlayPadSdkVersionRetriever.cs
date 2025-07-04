using System;
using System.Linq;
using JetBrains.Annotations;

namespace ElympicsPlayPad.Utility
{
    [PublicAPI]
    public static class PlayPadSdkVersionRetriever
    {
        private const string ElympicsName = "elympicsplaypad";

        public static Version GetVersionFromAssembly()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            return assemblies.Select(x => x.GetName()).FirstOrDefault(x => x.Name.ToLowerInvariant() == ElympicsName)?.Version;
        }

        public static string GetVersionStringFromAssembly() => GetVersionFromAssembly()?.ToString(3) ?? string.Empty;
    }
}
