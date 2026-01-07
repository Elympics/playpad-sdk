#nullable enable
using System;
using System.IO;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Elympics;
using ElympicsPlayPad.Editor.Editor.Util;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ElympicsPlayPad.Editor.Editor.UI
{
    public class PlayPadExtensionEditorWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset visualTree = null!;

        private const string RegistryPath = @"HKEY_CURRENT_USER\Software\Google\Chrome\NativeMessagingHosts\com.elympics.playpad";
        private const string Company = "Elympics";
        private const string ManifestFileName = "com.elympics.playpad.json";
        private const string MacOsNativeMessagingPath = "Library/Application Support/Google/Chrome/NativeMessagingHosts";
        private const string ManifestNameKey = "com.elympics.playpad";

        private static readonly string ManifestTemplateFilePath =
            $"Editor{Path.DirectorySeparatorChar}ExtensionProtocol{Path.DirectorySeparatorChar}Plugins{Path.DirectorySeparatorChar}com.elympics.playpad.json";

        private static readonly string WinHostExecutableFilePath =
            $"Editor{Path.DirectorySeparatorChar}ExtensionProtocol{Path.DirectorySeparatorChar}Plugins{Path.DirectorySeparatorChar}playpad-host.exe";

        private static readonly string MacOsArm64HostExecutableFilePath =
            $"Editor{Path.DirectorySeparatorChar}ExtensionProtocol{Path.DirectorySeparatorChar}Plugins{Path.DirectorySeparatorChar}playpad-host.osx-arm64";

        private static readonly string MacOsx64HostExecutableFilePath =
            $"Editor{Path.DirectorySeparatorChar}ExtensionProtocol{Path.DirectorySeparatorChar}Plugins{Path.DirectorySeparatorChar}playpad-host.osx-x64";

        private static readonly string ManifestRootFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private Button _installButton = null!;
        private Label _unsupportedSystemLabel = null!;
        private Label _readyToUseLabel = null!;
        private UniTaskCompletionSource<string>? _authenticationTask;
#pragma warning disable IDE0025
        private static bool IsWindows
        {
            get
            {
#if UNITY_EDITOR_WIN
                return true;
#else
                return false;
#endif
            }
        }

        private static bool IsMacOs
        {
            get
            {
#if UNITY_EDITOR_OSX
                return true;
#endif
                return false;
            }
        }

        private static bool IsSupportedPlatform
        {
            get
            {
#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX
                return true;
#else
                return false;
#endif
            }
        }
#pragma warning restore IDE0025

        [MenuItem("Tools/PlayPad/PlayPad Extension Installation Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<PlayPadExtensionEditorWindow>();
            window.titleContent = new GUIContent("PlayPad Extension Connector");
        }

        [MenuItem("Tools/PlayPad/ClearAuth")]
        public static void ClearAuth()
        {
            PlayerPrefs.DeleteKey(PlayPadExtensionAuthentication.AUTHORIZED_GAMEID_KEY);
            PlayerPrefs.DeleteKey(PlayPadExtensionAuthentication.AUTHORIZED_GAMEVERSIONID_KEY);
            PlayerPrefs.DeleteKey(PlayPadExtensionAuthentication.AUTHORIZED_DEVELOPER_JWT_KEY);
            PlayerPrefs.DeleteKey(PlayPadExtensionAuthentication.EXPIRATION_KEY);
        }

        private void CreateGUI()
        {
            Debug.Log("CreateGUI");
            var root = rootVisualElement;
            var inspector = visualTree.Instantiate();
            root.Add(inspector);
            var controlPanel = inspector.Q("control_panel");
            _installButton = controlPanel.Q<Button>("install_button");
            _readyToUseLabel = controlPanel.Q<Label>("ready_to_use_label");
            _unsupportedSystemLabel = inspector.Q<Label>("unsupported_platform_label");
            RefreshGui();
        }

        private void RefreshGui()
        {
            _installButton.UnregisterCallback<ClickEvent>(OnInstallClick);
            _readyToUseLabel.visible = false;

            if (!IsSupportedPlatform)
            {
                _unsupportedSystemLabel.visible = true;
                _installButton.visible = false;
                return;
            }

            _unsupportedSystemLabel.visible = false;

            var isInstalled = CheckManifestRegistration();
            if (isInstalled is false)
            {
                _installButton.visible = true;
                _installButton.RegisterCallback<ClickEvent>(OnInstallClick);
                return;
            }
            _installButton.visible = false;
            _readyToUseLabel.visible = true;
        }

        #region OnClickHandlers

        private void OnInstallClick(ClickEvent evt)
        {
            Debug.Log("Install manifest.");
            RegisterManifest();
            RefreshGui();
        }

        #endregion

        private void RegisterManifest()
        {
            if (IsWindows)
            {
                RegisterManifestOnWindows();
                return;
            }
            if (IsMacOs)
                RegisterManifestOnMacOs();
        }

        private bool CheckManifestRegistration()
        {
            if (IsWindows)
                return CheckManifestOnWindows();
            if (IsMacOs)
                return CheckManifestOnMacOs();
            return false;
        }

        #region Windows

        private bool CheckManifestOnWindows()
        {
            try
            {
                var registryValue = (string)Registry.GetValue(RegistryPath, string.Empty, null);
                if (string.IsNullOrEmpty(registryValue))
                    return false;

                var isManifestRegistered = string.Equals(registryValue, WinManifestFileAbsolutePath());
                if (isManifestRegistered is false)
                    return false;

                var manifestJson = GetManifestJson(WinManifestFileAbsolutePath());
                if (string.IsNullOrEmpty(manifestJson))
                    return false;

                var manifestObject = JObject.Parse(manifestJson);

                if (!ValidateManifest(manifestObject))
                    return false;

                var nameAsString = manifestObject["name"]?.Value<string>();
                if (string.IsNullOrEmpty(nameAsString))
                    return false;

                if (!string.Equals(nameAsString, ManifestNameKey))
                    return false;

                var pathAsString = manifestObject["path"]?.Value<string>();

                if (string.IsNullOrEmpty(pathAsString))
                    return false;

                if (!string.Equals(pathAsString, WinHostExecutableAbsolutePath()))
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error checking Windows manifest: {ex.Message}");
                return false;
            }
        }

        private void RegisterManifestOnWindows()
        {
            try
            {
                Registry.SetValue(RegistryPath, "", WinManifestFileAbsolutePath());
                var manifestTemplateJson = GetManifestTemplateJson();
                var jObj = JObject.Parse(manifestTemplateJson);
                jObj["path"] = WinHostExecutableAbsolutePath();
                jObj["name"] = ManifestNameKey;
                var result = TryCreateManifestFile(WinManifestFileAbsolutePath(), jObj.ToString());
                if (result)
                    Debug.Log("Windows manifest registered successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to register Windows manifest: {ex.Message}");
                _ = EditorUtility.DisplayDialog("Error", $"Failed to register manifest on Windows:\n{ex.Message}", "OK");
            }
        }

        private void RegisterManifestOnMacOs()
        {
            try
            {
                var manifestPath = MacOsManifestFileAbsolutePath();
                var manifestTemplateJson = GetManifestTemplateJson();
                var jObj = JObject.Parse(manifestTemplateJson);
                jObj["path"] = MacOsHostExecutableAbsolutePath();
                jObj["name"] = ManifestNameKey;
                var result = TryCreateManifestFile(manifestPath, jObj.ToString());
                if (result)
                    Debug.Log("macOS manifest registered successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to register macOS manifest: {ex.Message}");
                _ = EditorUtility.DisplayDialog("Error", $"Failed to register manifest on macOS:\n{ex.Message}", "OK");
            }
        }

        #endregion


        #region macOS

        private bool CheckManifestOnMacOs()
        {
            try
            {
                var manifestPath = MacOsManifestFileAbsolutePath();
                if (!File.Exists(manifestPath))
                    return false;

                var manifestJson = GetManifestJson(manifestPath);
                if (string.IsNullOrEmpty(manifestJson))
                    return false;

                var manifestObject = JObject.Parse(manifestJson);

                if (!ValidateManifest(manifestObject))
                    return false;

                var path = manifestObject["path"];
                var pathAsString = path?.Value<string>();

                if (string.IsNullOrEmpty(pathAsString))
                    return false;

                if (!string.Equals(pathAsString, MacOsHostExecutableAbsolutePath()))
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error checking macOS manifest: {ex.Message}");
                return false;
            }
        }

        #endregion

        private bool ValidateManifest(JObject manifestObject)
        {
            try
            {
                // Check required fields according to Chrome Native Messaging spec
                var nameField = manifestObject["name"]?.Value<string>();
                if (string.IsNullOrEmpty(nameField))
                {
                    Debug.LogError("Manifest missing required 'name' field");
                    return false;
                }

                var description = manifestObject["description"]?.Value<string>();
                if (string.IsNullOrEmpty(description))
                {
                    Debug.LogError("Manifest missing required 'description' field");
                    return false;
                }

                var type = manifestObject["type"]?.Value<string>();
                if (!string.Equals(type, "stdio", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogError($"Manifest 'type' field must be 'stdio', found: '{type}'");
                    return false;
                }

                var allowedOrigins = manifestObject["allowed_origins"];
                if (allowedOrigins == null || !allowedOrigins.HasValues)
                {
                    Debug.LogError("Manifest missing required 'allowed_origins' field or it's empty");
                    return false;
                }

                var path = manifestObject["path"]?.Value<string>();
                if (string.IsNullOrEmpty(path))
                {
                    Debug.LogError("Manifest missing required 'path' field");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error validating manifest: {ex.Message}");
                return false;
            }
        }

        private string GetManifestTemplateJson()
        {
            var manifestAssetPath = AssetPath(ManifestTemplateFilePath);
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(manifestAssetPath);
            if (asset)
                return asset.text;

            throw new ElympicsException("No asset found at path: " + manifestAssetPath);
        }
        private string GetManifestJson(string manifestPath)
        {
            try
            {
                using var reader = new StreamReader(manifestPath);
                return reader.ReadToEnd();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to open manifest file {manifestPath}. Message: {e.Message}");
                return string.Empty;
            }
        }
        private bool TryCreateManifestFile(string manifestPath, string manifest)
        {
            try
            {
                var directory = Path.GetDirectoryName(manifestPath);
                _ = Directory.CreateDirectory(directory!);
                using var writer = new StreamWriter(manifestPath);
                writer.Write(manifest);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write to manifest {manifestPath}. Message: {e.Message}");
                return false;
            }
        }
        private static string AssetPath(string assetPackagePath)
        {
            var rootPath = PathUtil.GetPackageRelativePath();
            return Path.Join(rootPath, assetPackagePath);
        }

        private static string AbsolutePath(string assetPackagePath)
        {
            var rootPath = PathUtil.GetPackageAbsolutePath();
            return Path.Join(rootPath, assetPackagePath);
        }
        private static string WinManifestFileAbsolutePath() => Path.Combine(ManifestRootFolder, Company, ManifestFileName);
        private static string WinHostExecutableAbsolutePath()
        {
            var hostExecutable = AbsolutePath(WinHostExecutableFilePath);
            return Path.Combine(PathUtil.FromUnityToSystem(PathUtil.GetDataPathWoAssets()), hostExecutable);
        }

        private static string MacOsManifestFileAbsolutePath()
        {
            var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(homeDirectory, MacOsNativeMessagingPath, ManifestFileName);
        }

        private static string MacOsHostExecutableAbsolutePath()
        {
            var exeFileForCurrentArchitecture = FetchMacOsExec();
            var hostExecutable = AbsolutePath(exeFileForCurrentArchitecture);
            return Path.Combine(PathUtil.FromUnityToSystem(PathUtil.GetDataPathWoAssets()), hostExecutable);
        }

        private static string FetchMacOsExec()
        {
            if (Application.platform != RuntimePlatform.OSXEditor)
                throw new ElympicsException("MacOS only works on Mac OS X");

            var osArch = RuntimeInformation.OSArchitecture;
            var processArch = RuntimeInformation.ProcessArchitecture;

            Debug.Log("OS Architecture: " + osArch);
            Debug.Log("Process Architecture: " + processArch);

            return processArch switch
            {
                Architecture.Arm64 => MacOsArm64HostExecutableFilePath,
                Architecture.X64 => MacOsx64HostExecutableFilePath,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
