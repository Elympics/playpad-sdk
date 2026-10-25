using System;
using System.Collections.Generic;
using System.Linq;
using Elympics.Editor.UnityCli;
using ElympicsPlayPad.Editor.Build;
using Unity.Pipeline.Commands;
using UnityEditor;
using TemplateAction = ElympicsPlayPad.Editor.Build.PlayPadWebGLTemplateInstaller.TemplateAction;

namespace ElympicsPlayPad.Editor.UnityCli
{
    public static class PlayPadTemplateCliCommands
    {
        internal const string TemplateNotFound = "ELYMPICS_PLAYPAD_TEMPLATE_NOT_FOUND";
        internal const string TemplateCustomized = "ELYMPICS_PLAYPAD_TEMPLATE_CUSTOMIZED";
        internal const string TemplateInstallFailed = "ELYMPICS_PLAYPAD_TEMPLATE_INSTALL_FAILED";
        internal const string TemplateValueNotSaved = "ELYMPICS_PLAYPAD_TEMPLATE_VALUE_NOT_SAVED";

        private static readonly (string Name, string Variable, string CustomValue)[] BuildValues =
        {
            Value("GAME_ID"), Value("GAME_NAME"), Value("BUILD_IDENTIFIER"), Value("PROTOCOL_VERSION"), Value("QUEUE"), Value("ENVIRONMENT"),
            ("REGION", PlayPadPreProcessBuild.RegionVariable, PlayPadPreProcessBuild.RegionCustomValue),
        };

        [CliCommand("playpad_template_status",
            "Report the ElympicsLobby WebGL template: whether Assets/WebGLTemplates/ElympicsLobby matches the PlayPad package's copy, "
            + "what an Editor load or a build would do with it, and whether Player Settings select it.",
            Tags = new[] { "elympics", "playpad", "read" })]
        public static object Status() => CliRun.Sync(() =>
        {
            var status = RequireStatus();
            return new
            {
                status = "ok",
                action = ToName(status.Action),
                installed = status.InstalledHash != null,
                customized = status.Action == TemplateAction.KeepCustomized,
                selected = IsTemplateSelected,
                selectedTemplate = PlayerSettings.WebGL.template,
                installedPath = status.InstalledPath,
                sourcePath = status.SourcePath,
                status.InstalledHash,
                status.StampHash,
                status.ShippedHash,
            };
        });

        [CliCommand("playpad_template_install",
            "Install the PlayPad package's ElympicsLobby WebGL template into Assets/WebGLTemplates, or update it. "
            + "Refuses to overwrite a customized copy unless --force true. Doesn't change which template Player Settings select.",
            Tags = new[] { "elympics", "playpad", "write" })]
        public static object Install(
            [CliArg("force", "Overwrite the installed template even when it was customized after PlayPad installed it.")] bool force = false) => CliRun.Sync(() =>
        {
            var before = RequireStatus();
            if (before.Action == TemplateAction.KeepCustomized && !force)
                throw new ElympicsCliException(TemplateCustomized, $"{before.InstalledPath} differs from the version PlayPad installed, so it was left as it is.",
                    "keep the customized copy, or repeat with --force true to replace it with the package's template.");
            if (before.Action == TemplateAction.UpToDate && !force)
                return new { status = "up_to_date", previousAction = ToName(before.Action), installedPath = before.InstalledPath, selected = IsTemplateSelected };

            if (!PlayPadWebGLTemplateInstaller.TryInstallTemplate())
                throw new ElympicsCliException(TemplateInstallFailed, $"Installing the {PlayPadWebGLTemplateInstaller.TemplateName} WebGL template failed.",
                    "check the Console (console --level error) for the reason, fix it and retry.");
            return new { status = "installed", previousAction = ToName(before.Action), installedPath = before.InstalledPath, selected = IsTemplateSelected };
        });

        [CliCommand("playpad_template_values",
            "Get the ElympicsLobby template custom values from Player Settings, or set ENVIRONMENT, REGION and QUEUE (empty arguments are left unchanged). "
            + "Builds fill the other values from the active game config, and TEMPLATE_* environment variables override all of them.",
            Tags = new[] { "elympics", "playpad", "write" })]
        public static object Values(
            [CliArg("environment", "Elympics environment the lobby connects to, e.g. prod or stg. Builds default to prod.")] string environment = "",
            [CliArg("region", "Elympics region the lobby plays in (elympics_regions_list lists the game's regions). WebGL builds with the template fail without one.")]
            string region = "",
            [CliArg("queue", "Matchmaking queue the lobby joins. Builds default to solo.")] string queue = "") => CliRun.Sync(() =>
        {
            var requested = new Dictionary<string, string> { ["ENVIRONMENT"] = environment, ["REGION"] = region, ["QUEUE"] = queue };
            var changes = requested.Where(pair => !string.IsNullOrEmpty(pair.Value))
                .Select(pair => (name: pair.Key, value: CliArgs.Require(pair.Value, pair.Key.ToLowerInvariant()))).ToList();

            var changed = new List<string>();
            foreach (var (name, trimmed) in changes)
            {
                var key = BuildValues.Single(value => value.Name == name).CustomValue;
                if (PlayerSettings.GetTemplateCustomValue(key) == trimmed)
                    continue;
                PlayerSettings.SetTemplateCustomValue(key, trimmed);
                if (PlayerSettings.GetTemplateCustomValue(key) != trimmed)
                    throw new ElympicsCliException(TemplateValueNotSaved, $"Player Settings didn't keep {key} = \"{trimmed}\".",
                        $"select the template first (PlayerSettings.WebGL.template = \"PROJECT:{PlayPadWebGLTemplateInstaller.TemplateName}\"; playpad_template_status shows it), then retry.");
                changed.Add(name.ToLowerInvariant());
            }
            if (changed.Count > 0)
                AssetDatabase.SaveAssets();

            var values = new Dictionary<string, string>();
            var environmentOverrides = new Dictionary<string, string>();
            foreach (var (name, variable, customValue) in BuildValues)
            {
                values[name.ToLowerInvariant()] = NullIfEmpty(PlayerSettings.GetTemplateCustomValue(customValue));
                var overridden = Environment.GetEnvironmentVariable(variable);
                if (!string.IsNullOrEmpty(overridden))
                    environmentOverrides[variable] = overridden;
            }

            return new
            {
                status = changed.Count > 0 ? "updated" : "ok",
                changed,
                selected = IsTemplateSelected,
                values,
                environmentOverrides,
            };
        });

        private static bool IsTemplateSelected => PlayerSettings.WebGL.template == PlayPadPreProcessBuild.ElympicsTemplateName;

        private static (string Name, string Variable, string CustomValue) Value(string name) => (name, "TEMPLATE_" + name, "ELYMPICS_" + name);

        private static string NullIfEmpty(string value) => string.IsNullOrEmpty(value) ? null : value;

        private static string ToName(TemplateAction action) => action switch
        {
            TemplateAction.UpToDate => "up_to_date",
            TemplateAction.Install => "install",
            TemplateAction.Refresh => "refresh",
            TemplateAction.KeepCustomized => "keep_customized",
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
        };

        private static PlayPadWebGLTemplateInstaller.TemplateStatus RequireStatus() =>
            PlayPadWebGLTemplateInstaller.GetStatus()
            ?? throw new ElympicsCliException(TemplateNotFound, "The PlayPad package's WebGL template wasn't found.",
                "check the Console (console --level error) for the path it looked in, and reinstall the com.daftmobile.elympics-playpad package if WebGLTemplates~ is missing.");
    }
}
