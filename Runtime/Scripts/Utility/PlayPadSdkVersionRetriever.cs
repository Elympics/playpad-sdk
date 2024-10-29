using System;
using System.Linq;

namespace ElympicsPlayPad.Utility
{
    public static class PlayPadSdkVersionRetriever
    {
        private const string ElympicsName = "elympicsplaypad";

        public static Version GetVersionFromAssembly()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            return assemblies.Select(x => x.GetName()).FirstOrDefault(x => x.Name.ToLowerInvariant() == ElympicsName)?.Version;
        }

        public static string GetVersionStringFromAssembly()
        {
            return GetVersionFromAssembly()?.ToString(3) ?? string.Empty;
        }
    }
}
