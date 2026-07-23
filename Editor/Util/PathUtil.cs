using System.IO;
using System.Reflection;
using UnityEditor.PackageManager;
using UnityEngine;

namespace ElympicsPlayPad.Editor.Editor.Util
{
    public static class PathUtil
    {
        public static string FromUnityToSystem(string path) => path.Replace('/', Path.DirectorySeparatorChar);

        public static string FromSystemToUnity(string path) => path.Replace('\\', '/');

        public static string GetPackageRelativePath()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var info = PackageInfo.FindForAssembly(assembly);
            if (info is null)
                return string.Empty;
            //#if UNITY_EDITOR_WIN
            var path = info.assetPath;
            //#elif UNITY_EDITOR_OSX
            //            var path = Path.GetFullPath(info.assetPath);
            //#endif
            return FromUnityToSystem(path);
        }

        public static string GetPackageAbsolutePath()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var info = PackageInfo.FindForAssembly(assembly);
            return info is null ? string.Empty : FromUnityToSystem(info.resolvedPath);
        }

        public static string GetDataPathWoAssets() => Directory.GetParent(Application.dataPath)?.FullName;
    }
}
