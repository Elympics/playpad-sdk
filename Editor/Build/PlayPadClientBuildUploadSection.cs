#nullable enable

using System;
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
            AddField(container, "Client version", ClientVersionTooltip, _clientVersion, value => _clientVersion = value);
            AddField(container, "Client build path", "Path to the directory containing the client build that will be uploaded.", clientBuildPath, value => clientBuildPath = value);

            var uploadButton = new Button(() => Upload(config, clientBuildPath)) { text = "Upload client" };
            uploadButton.AddToClassList("elympics-wide-button");
            container.Add(uploadButton);
        }

        private static void AddField(VisualElement container, string label, string tooltip, string value, Action<string> setValue)
        {
            var row = new VisualElement();
            row.AddToClassList("elympics-field-row");
            var labelElement = new Label(label) { tooltip = tooltip };
            labelElement.AddToClassList("elympics-field-label");
            var field = new TextField();
            field.AddToClassList("elympics-endpoint-field");
            field.SetValueWithoutNotify(value);
            _ = field.RegisterValueChangedCallback(evt => setValue(evt.newValue));
            row.Add(labelElement);
            row.Add(field);
            container.Add(row);
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
