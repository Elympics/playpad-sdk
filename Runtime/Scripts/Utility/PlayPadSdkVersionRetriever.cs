using System;
using System.Linq;
using Elympics;
using JetBrains.Annotations;

namespace ElympicsPlayPad.Utility
{
    [PublicAPI]
    public static class PlayPadSdkVersionRetriever
    {
        private const string PlayPadAssemblyName = "ElympicsPlayPad";

        public static Version GetVersionFromAssembly()
        {
            var assemblies = LoadedAssemblies.Get();
            return assemblies.Select(x => x.GetName()).FirstOrDefault(x => x.Name == PlayPadAssemblyName)?.Version;
        }

        public static string GetVersionStringFromAssembly() => GetVersionFromAssembly()?.ToString(3) ?? string.Empty;
    }
}
