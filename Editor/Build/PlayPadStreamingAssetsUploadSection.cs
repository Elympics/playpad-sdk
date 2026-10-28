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

        private const string LayoutTooltip = "Addressable-based variants: one folder per variant, each an Addressables content build with its own \"aa\" folder. "
            + "Unstructured assets: everything below the path is uploaded as-is, without validation.";
        private const string VersionTooltip = "Must match the version the content was built against. Independent of the game version. Max 63 characters; only alphanumerics, \"-\" and \".\".";
        private const string PathTooltip = "Local folder to upload, subfolders included. For Addressables, the content build output directory (e.g. ServerData), not Assets/StreamingAssets.";

        private static readonly StreamingAssetsLayout[] Layouts = (StreamingAssetsLayout[])Enum.GetValues(typeof(StreamingAssetsLayout));

        private string _path = EditorPrefs.GetString(PathKey, string.Empty);
        private string _version = EditorPrefs.GetString(VersionKey, string.Empty);
        private StreamingAssetsLayout _layout = LoadLayout();

        public int Order => 1;

        [InitializeOnLoadMethod]
        private static void Register() => ManageGamesWindowSections.Register(new PlayPadStreamingAssetsUploadSection());

        public void CreateGUI(VisualElement container, ElympicsConfig config)
        {
            container.AddSectionSpacer();
            var info = new Label("Upload StreamingAssets content under its own content version, independent of client builds.");
            info.AddToClassList("elympics-info");
            info.AddToClassList("elympics-centered");
            container.Add(info);

            var choices = Layouts.Select(GetLayoutLabel).ToList();
            var layoutField = new DropdownField(choices, Array.IndexOf(Layouts, _layout));
            _ = layoutField.RegisterValueChangedCallback(_ => _layout = Layouts[layoutField.index]);
            container.AddRow("Content layout", LayoutTooltip, layoutField);
            container.AddTextField("Content version", VersionTooltip, _version, value => _version = value);
            container.AddTextField("Content path", PathTooltip, _path, value => _path = value);

            var uploadButton = new Button(() => Upload(config)) { text = "Upload StreamingAssets" };
            uploadButton.AddToClassList("elympics-wide-button");
            container.Add(uploadButton);
        }

        private static StreamingAssetsLayout LoadLayout()
        {
            var storedLayout = EditorPrefs.GetInt(LayoutKey, (int)StreamingAssetsLayout.AddressableVariants);
            return Enum.IsDefined(typeof(StreamingAssetsLayout), storedLayout) ? (StreamingAssetsLayout)storedLayout : StreamingAssetsLayout.AddressableVariants;
        }

        private static string GetLayoutLabel(StreamingAssetsLayout layout) => layout switch
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

            ElympicsWebIntegration.UploadStreamingAssets(activeGameConfig.GameId, _path, _version, _layout);
        }
    }
}
