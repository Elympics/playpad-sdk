using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace ElympicsPlayPad.Editor.Build
{
    /// <summary>
    /// Keeps <c>Assets/WebGLTemplates/ElympicsLobby</c> in line with the template shipped in the package.
    /// The installed copy's version is the content hash recorded in a stamp file next to it, outside the template folder
    /// so it never ends up in builds. A copy that no longer matches its stamp was edited in the project and is kept.
    /// </summary>
    [InitializeOnLoad]
    internal static class PlayPadWebGLTemplateInstaller
    {
        internal const string TemplateName = "ElympicsLobby";
        private const string SourceFolder = "WebGLTemplates~";
        private const string StampFileName = TemplateName + ".installed.json";
        private const string CustomizedWarningShownKey = "ElympicsPlayPad.WebGLTemplate.CustomizedWarningShown";
        private const string ReinstallMenuPath = "Tools/PlayPad/Reinstall WebGL Template";

        /// <summary>Hashes of templates shipped before installed copies were stamped, so those copies can be refreshed.</summary>
        internal static readonly IReadOnlyCollection<string> UnstampedShippedHashes = new[]
        {
            "a4ad21a1ff51ac65fb124b00a644b18e7d3bd47b3a81013f1856c07c0e89f08a", // template v0.1.5
        };

        private static readonly HashSet<string> TextExtensions = new(StringComparer.OrdinalIgnoreCase) { ".html", ".js", ".css", ".json", ".txt", ".svg", ".map" };

        internal enum TemplateAction
        {
            UpToDate,
            Install,
            Refresh,
            KeepCustomized,
        }

        [Serializable]
        private class Stamp
        {
            public string hash;
            public string package;
        }

        static PlayPadWebGLTemplateInstaller() => EditorApplication.delayCall += UpdateTemplateIfNeeded;

        private static string InstalledPath => Path.Combine(Application.dataPath, "WebGLTemplates", TemplateName);
        private static string StampPath => Path.Combine(Application.dataPath, "WebGLTemplates", StampFileName);

        [MenuItem(ReinstallMenuPath)]
        public static void ReinstallTemplate()
        {
            if (TryInstallTemplate())
                Debug.Log("Elympics PlayPad WebGL template reinstalled.");
            else
                Debug.LogWarning("Failed to install Elympics PlayPad WebGL template.");
        }

        internal static void UpdateTemplateIfNeeded()
        {
            if (!TryGetSourcePath(out var sourcePath))
                return;

            var installedPath = InstalledPath;
            var installedHash = File.Exists(Path.Combine(installedPath, "index.html")) ? ComputeHash(installedPath) : null;
            var stampHash = ReadStampHash();
            var action = Decide(installedHash, stampHash, ComputeHash(sourcePath), UnstampedShippedHashes);
            switch (action)
            {
                case TemplateAction.Install:
                case TemplateAction.Refresh:
                    Debug.Log(action == TemplateAction.Install
                        ? "Installing ElympicsLobby WebGL template."
                        : "Updating the ElympicsLobby WebGL template to the version in the package.");
                    _ = TryInstallTemplate();
                    break;
                case TemplateAction.UpToDate:
                    if (stampHash != installedHash)
                        WriteStamp(installedHash);
                    break;
                case TemplateAction.KeepCustomized:
                    if (SessionState.GetBool(CustomizedWarningShownKey, false))
                        break;
                    SessionState.SetBool(CustomizedWarningShownKey, true);
                    Debug.LogWarning($"Assets/WebGLTemplates/{TemplateName} differs from the version PlayPad installed, so it was left as it is "
                        + $"and doesn't get template updates. To replace it with the package's template, use {ReinstallMenuPath}.");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        /// <param name="installedHash">Hash of the installed copy, or null when it's missing or has no index.html.</param>
        /// <param name="stampHash">Hash recorded when PlayPad installed the copy, or null for copies installed before stamping.</param>
        internal static TemplateAction Decide(string installedHash, string stampHash, string shippedHash, IReadOnlyCollection<string> unstampedShippedHashes)
        {
            if (installedHash == null)
                return TemplateAction.Install;
            if (installedHash == shippedHash)
                return TemplateAction.UpToDate;
            var unmodified = stampHash != null ? installedHash == stampHash : unstampedShippedHashes.Contains(installedHash);
            return unmodified ? TemplateAction.Refresh : TemplateAction.KeepCustomized;
        }

        /// <summary>
        /// SHA-256 over the relative paths and contents of all files except <c>.meta</c> and hidden ones.
        /// CR bytes are dropped from text files, because git may check the package out with either line ending.
        /// </summary>
        internal static string ComputeHash(string directory)
        {
            var root = Path.GetFullPath(directory);
            var files = Directory.GetFiles(root, "*", SearchOption.AllDirectories)
                .Where(path => !path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase) && !Path.GetFileName(path).StartsWith("."))
                .Select(path => (path, relative: Path.GetRelativePath(root, path).Replace('\\', '/')))
                .OrderBy(file => file.relative, StringComparer.Ordinal);

            using var sha = SHA256.Create();
            var separator = new byte[] { 0 };
            foreach (var (path, relative) in files)
            {
                var content = File.ReadAllBytes(path);
                if (TextExtensions.Contains(Path.GetExtension(path)))
                    content = content.Where(b => b != (byte)'\r').ToArray();
                var name = Encoding.UTF8.GetBytes(relative);
                _ = sha.TransformBlock(name, 0, name.Length, null, 0);
                _ = sha.TransformBlock(separator, 0, 1, null, 0);
                _ = sha.TransformBlock(content, 0, content.Length, null, 0);
                _ = sha.TransformBlock(separator, 0, 1, null, 0);
            }
            _ = sha.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            return string.Concat(sha.Hash.Select(b => b.ToString("x2")));
        }

        private static bool TryInstallTemplate()
        {
            if (!TryGetSourcePath(out var sourcePath))
                return false;

            // Deleting first drops files the new version no longer has, e.g. the previous hashed bundle names.
            var installedPath = InstalledPath;
            if (Directory.Exists(installedPath))
                Directory.Delete(installedPath, true);
            CopyDirectory(sourcePath, installedPath);
            WriteStamp(ComputeHash(installedPath));
            SessionState.EraseBool(CustomizedWarningShownKey);
            AssetDatabase.Refresh();
            return true;
        }

        private static bool TryGetSourcePath(out string sourcePath)
        {
            sourcePath = null;
            var packagePath = PackageInfo.FindForAssembly(Assembly.GetExecutingAssembly())?.resolvedPath;
            if (string.IsNullOrEmpty(packagePath))
            {
                Debug.LogError("Could not resolve Elympics PlayPad package path.");
                return false;
            }

            sourcePath = Path.Combine(packagePath, SourceFolder, TemplateName);
            if (Directory.Exists(sourcePath))
                return true;
            Debug.LogError($"WebGL template source not found at: {sourcePath}");
            return false;
        }

        private static string ReadStampHash()
        {
            try
            {
                return File.Exists(StampPath) ? JsonUtility.FromJson<Stamp>(File.ReadAllText(StampPath))?.hash : null;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private static void WriteStamp(string hash)
        {
            var package = PackageInfo.FindForAssembly(Assembly.GetExecutingAssembly());
            var stamp = new Stamp { hash = hash, package = package != null ? $"{package.name}@{package.version}" : "" };
            File.WriteAllText(StampPath, JsonUtility.ToJson(stamp, true) + "\n");
        }

        private static void CopyDirectory(string source, string destination)
        {
            _ = Directory.CreateDirectory(destination);

            foreach (var file in Directory.GetFiles(source))
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), true);

            foreach (var dir in Directory.GetDirectories(source))
                CopyDirectory(dir, Path.Combine(destination, new DirectoryInfo(dir).Name));
        }
    }
}
