#nullable enable

using Elympics;
using Elympics.Editor.Config;
using UnityEditor;
using UnityEngine.UIElements;

namespace ElympicsPlayPad.Editor.Build
{
    internal class PlayPadClientBuildUploadSection : IManageGameSection
    {
        private const string ClientBuildPathKey = "ElympicsClientBuildPathForUpload";

        private const string ClientVersionTooltip = "Name of the client build version that will be uploaded. It doesn't have to match the game version from Elympics game config. "
            + "The game version from Elympics game config is used to match client with server version, so you can upload a new client version with UI fixes without uploading a new server version.";

        private string? _clientVersion;

        public int Order => 0;

        [InitializeOnLoadMethod]
        private static void Register() => ManageGamesWindowSections.Register(new PlayPadClientBuildUploadSection());

        public void CreateGUI(VisualElement container, ElympicsConfig config)
        {
            _clientVersion ??= config.GetCurrentGameConfig()?.GameVersion ?? string.Empty;
            var clientBuildPath = EditorPrefs.GetString(ClientBuildPathKey, string.Empty);

            container.Add(new VisualElement { style = { height = 9 } });
            ManageGameSectionRows.AddTextField(container, "Client version", ClientVersionTooltip, _clientVersion, value => _clientVersion = value);
            ManageGameSectionRows.AddTextField(container, "Client build path", "Path to the directory containing the client build that will be uploaded.", clientBuildPath,
                value => clientBuildPath = value);

            var uploadButton = new Button(() => Upload(config, clientBuildPath)) { text = "Upload client" };
            uploadButton.AddToClassList("elympics-wide-button");
            container.Add(uploadButton);
        }

        private void Upload(ElympicsConfig config, string clientBuildPath)
        {
            var activeGameConfig = config.GetCurrentGameConfig();
            if (activeGameConfig == null)
                return;

            EditorPrefs.SetString(ClientBuildPathKey, clientBuildPath);

            if (!ElympicsWebIntegration.IsConnectedToElympics())
                return;

            ElympicsWebIntegration.UploadClientBuild(clientBuildPath, activeGameConfig.GameId, _clientVersion ?? string.Empty, activeGameConfig.GameVersion);
        }
    }
}
