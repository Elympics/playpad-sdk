using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace ElympicsPlayPad.Editor.Build
{
    [InitializeOnLoad]
    internal static class PlayPadWebGLTemplateInstaller
    {
        private const string TemplateName = "ElympicsLobby";
        private const string SourceFolder = "WebGLTemplates~";

        static PlayPadWebGLTemplateInstaller() => EditorApplication.delayCall += InstallTemplateIfNeeded;

        [MenuItem("Tools/PlayPad/Install WebGL Template")]
        public static void InstallTemplate()
        {
            var destinationPath = Path.Combine(Application.dataPath, "WebGLTemplates", TemplateName);
            if (Directory.Exists(destinationPath))
            {
                Debug.Log("Elympics PlayPad WebGL template is already installed.");
                return;
            }

            if (TryInstallTemplate())
                Debug.Log("Elympics PlayPad WebGL template installed successfully.");
            else
                Debug.LogWarning("Failed to install Elympics PlayPad WebGL template.");
        }

        private static void InstallTemplateIfNeeded()
        {
            var destinationPath = Path.Combine(Application.dataPath, "WebGLTemplates", TemplateName);
            if (Directory.Exists(destinationPath))
                return;

            _ = TryInstallTemplate();
        }

        private static bool TryInstallTemplate()
        {
            Debug.LogError("Installing ElympicsLobby WebGL template.");
            var packagePath = GetPackageResolvedPath();
            if (string.IsNullOrEmpty(packagePath))
            {
                Debug.LogError("Could not resolve Elympics PlayPad package path.");
                return false;
            }

            var sourcePath = Path.Combine(packagePath, SourceFolder, TemplateName);
            if (!Directory.Exists(sourcePath))
            {
                Debug.LogError($"WebGL template source not found at: {sourcePath}");
                return false;
            }

            var destinationPath = Path.Combine(Application.dataPath, "WebGLTemplates", TemplateName);
            CopyDirectory(sourcePath, destinationPath);
            AssetDatabase.Refresh();
            return true;
        }

        private static string GetPackageResolvedPath()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var info = PackageInfo.FindForAssembly(assembly);
            return info?.resolvedPath;
        }

        private static void CopyDirectory(string source, string destination)
        {
            _ = Directory.CreateDirectory(destination);

            foreach (var file in Directory.GetFiles(source))
            {
                var destFile = Path.Combine(destination, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (var dir in Directory.GetDirectories(source))
            {
                var destDir = Path.Combine(destination, new DirectoryInfo(dir).Name);
                CopyDirectory(dir, destDir);
            }
        }
    }
}
