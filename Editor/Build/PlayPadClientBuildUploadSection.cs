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

        private const string ClientVersionTooltip = "Name of the uploaded client version. It doesn't have to match the game version from the game config, "
            + "which is used to match the client with the server version.";

        private string? _gameId;
        private string _clientVersion = string.Empty;
        private string _clientBuildPath = string.Empty;

        public int Order => 0;

        [InitializeOnLoadMethod]
        private static void Register() => ManageGamesWindowSections.Register(new PlayPadClientBuildUploadSection());

        public void CreateGUI(VisualElement container, ElympicsConfig config)
        {
            var game = config.GetCurrentGameConfig();
            if (_gameId == null || _gameId != game?.GameId)
            {
                _gameId = game?.GameId ?? string.Empty;
                _clientVersion = game?.GameVersion ?? string.Empty;
                _clientBuildPath = EditorPrefs.GetString(ClientBuildPathKey, string.Empty);
            }

            container.AddSectionSpacer();
            container.AddTextField("Client version", ClientVersionTooltip, _clientVersion, value => _clientVersion = value);
            container.AddTextField("Client build path", "Directory containing the client build to upload.", _clientBuildPath,
                value => _clientBuildPath = value);

            var uploadButton = new Button(() => Upload(config)) { text = "Upload client" };
            uploadButton.AddToClassList("elympics-wide-button");
            container.Add(uploadButton);
        }

        private void Upload(ElympicsConfig config)
        {
            var activeGameConfig = config.GetCurrentGameConfig();
            if (activeGameConfig == null)
                return;

            EditorPrefs.SetString(ClientBuildPathKey, _clientBuildPath);

            if (!ElympicsWebIntegration.IsConnectedToElympics())
                return;

            ElympicsWebIntegration.UploadClientBuild(_clientBuildPath, activeGameConfig.GameId, _clientVersion, activeGameConfig.GameVersion);
        }
    }
}
