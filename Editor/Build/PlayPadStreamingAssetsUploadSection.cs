#nullable enable

using System;
using System.Linq;
using Elympics;
using Elympics.Editor;
using Elympics.Editor.Config;
using UnityEditor;
using UnityEngine.UIElements;

namespace ElympicsPlayPad.Editor.Build
{
    internal class PlayPadStreamingAssetsUploadSection : IManageGameSection
    {
        private const string PathKey = "ElympicsStreamingAssetsPathForUpload";
        private const string VersionKey = "ElympicsStreamingAssetsVersionForUpload";
        private const string LayoutKey = "ElympicsStreamingAssetsLayoutForUpload";

        private const string LayoutTooltip = "Directory of Addressable-based variants expects one folder per variant, each an Addressables content build with its own \"aa\" folder, "
            + "and also uploads a generated variants.meta.json listing them. Files sitting directly in the path are uploaded too, with a warning. "
            + "Unstructured assets validates nothing and just uploads everything below the path as-is.";
        private const string VersionTooltip = "How this content is addressed later, so it must match the version the content was built against. "
            + "Independent of the game version. Max 63 characters; only alphanumerics, \"-\" and \".\".";
        private const string PathTooltip = "Local folder to upload. Subfolders are included. For Addressables content this is the content build output directory (e.g. ServerData), "
            + "not Assets/StreamingAssets - the Addressables variant builder deletes that folder on every build.";

        private static readonly StreamingAssetsLayout[] Layouts = (StreamingAssetsLayout[])Enum.GetValues(typeof(StreamingAssetsLayout));

        private string? _path;
        private string? _version;
        private StreamingAssetsLayout _layout;

        public int Order => 1;

        [InitializeOnLoadMethod]
        private static void Register() => ManageGamesWindowSections.Register(new PlayPadStreamingAssetsUploadSection());

        public void CreateGUI(VisualElement container, ElympicsConfig config)
        {
            if (_path == null)
            {
                _path = EditorPrefs.GetString(PathKey, string.Empty);
                _version = EditorPrefs.GetString(VersionKey, string.Empty);
                var storedLayout = EditorPrefs.GetInt(LayoutKey, (int)StreamingAssetsLayout.AddressableVariants);
                _layout = Enum.IsDefined(typeof(StreamingAssetsLayout), storedLayout) ? (StreamingAssetsLayout)storedLayout : StreamingAssetsLayout.AddressableVariants;
            }

            container.Add(new VisualElement { style = { height = 9 } });
            var info = new Label("Upload StreamingAssets content under its own content version, independent of client builds.");
            info.AddToClassList("elympics-info");
            info.AddToClassList("elympics-centered");
            container.Add(info);

            var choices = Layouts.Select(LayoutLabel).ToList();
            var layoutField = new DropdownField(choices, Array.IndexOf(Layouts, _layout));
            _ = layoutField.RegisterValueChangedCallback(evt =>
            {
                var index = choices.IndexOf(evt.newValue);
                if (index >= 0)
                    _layout = Layouts[index];
            });
            ManageGameSectionRows.AddRow(container, "Content layout", LayoutTooltip, layoutField);
            ManageGameSectionRows.AddTextField(container, "Content version", VersionTooltip, _version ?? string.Empty, value => _version = value);
            ManageGameSectionRows.AddTextField(container, "Content path", PathTooltip, _path, value => _path = value);

            var uploadButton = new Button(() => Upload(config)) { text = "Upload StreamingAssets" };
            uploadButton.AddToClassList("elympics-wide-button");
            container.Add(uploadButton);
        }

        private static string LayoutLabel(StreamingAssetsLayout layout) => layout switch
        {
            StreamingAssetsLayout.AddressableVariants => "Directory of Addressable-based variants",
            StreamingAssetsLayout.UnstructuredAssets => "Unstructured assets",
            _ => layout.ToString(),
        };

        private void Upload(ElympicsConfig config)
        {
            var activeGameConfig = config.GetCurrentGameConfig();
            if (activeGameConfig == null)
                return;

            EditorPrefs.SetString(PathKey, _path);
            EditorPrefs.SetString(VersionKey, _version);
            EditorPrefs.SetInt(LayoutKey, (int)_layout);

            if (!ElympicsWebIntegration.IsConnectedToElympics())
                return;

            ElympicsWebIntegration.UploadStreamingAssets(activeGameConfig.GameId, _path ?? string.Empty, _version ?? string.Empty, _layout);
        }
    }
}
