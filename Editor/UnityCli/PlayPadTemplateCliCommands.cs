#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
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
        internal const string TemplateUnrecognized = "ELYMPICS_PLAYPAD_TEMPLATE_UNRECOGNIZED";
        internal const string TemplateInstallFailed = "ELYMPICS_PLAYPAD_TEMPLATE_INSTALL_FAILED";
        internal const string TemplateValueNotSaved = "ELYMPICS_PLAYPAD_TEMPLATE_VALUE_NOT_SAVED";

        [CliCommand("playpad_get_template_status",
            "Report the ElympicsLobby WebGL template: whether Assets/WebGLTemplates/ElympicsLobby matches the PlayPad package's copy, "
            + "what an Editor load or a build would do with it, and whether Player Settings select it.",
            Tags = new[] { "elympics/playpad/template" })]
        public static object GetTemplateStatus() => CliRun.Run(() =>
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
                installedHash = status.InstalledHash,
                stampHash = status.StampHash,
                shippedHash = status.ShippedHash,
            };
        });

        [CliCommand("playpad_install_template",
            "Install the PlayPad package's ElympicsLobby WebGL template into Assets/WebGLTemplates, or update it. "
            + "Refuses to overwrite a customized copy or an unrecognised non-empty folder unless --force true. Doesn't change which template Player Settings select.",
            Tags = new[] { "elympics/playpad/template" })]
        public static object InstallTemplate(
            [CliArg("force", "Overwrite the installed template even when it was customized or the folder isn't recognised.")] bool force = false) => CliRun.Run(() =>
        {
            var before = RequireStatus();
            if (before.Action == TemplateAction.KeepCustomized && !force)
                throw new ElympicsCliException(TemplateCustomized, $"{before.InstalledPath} differs from the version PlayPad installed, so it was left as it is.",
                    "keep the customized copy, or repeat with --force true to replace it with the package's template.");
            if (before.Action == TemplateAction.UpToDate && !force)
                return CreateResult("up_to_date");

            if (before.Action == TemplateAction.Install && !force && IsNonEmptyDirectory(before.InstalledPath))
                throw new ElympicsCliException(TemplateUnrecognized, $"{before.InstalledPath} isn't empty and has no index.html, so it isn't a template PlayPad can recognise.",
                    "move or delete the folder, or repeat with --force true to overwrite it with the package's template.");

            const string installFailedFix = "check the Console (console --level error) for the reason, fix it and retry.";
            bool installed;
            try
            {
                installed = PlayPadWebGLTemplateInstaller.TryInstallTemplate();
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                throw new ElympicsCliException(TemplateInstallFailed, $"Installing the {PlayPadWebGLTemplateInstaller.TemplateName} WebGL template failed: {e.Message}",
                    "close programs that use the files in Assets/WebGLTemplates, check their permissions, and retry.");
            }
            if (!installed)
                throw new ElympicsCliException(TemplateInstallFailed, $"Installing the {PlayPadWebGLTemplateInstaller.TemplateName} WebGL template failed.", installFailedFix);
            return CreateResult("installed");

            object CreateResult(string status) => new { status, previousAction = ToName(before.Action), installedPath = before.InstalledPath, selected = IsTemplateSelected };
        });

        [CliCommand(PlayPadPreProcessBuild.GetTemplateValuesCommand,
            "Get the ElympicsLobby template custom values from Player Settings and the TEMPLATE_* environment variables that override them. "
            + "Builds fill GAME_ID, GAME_NAME, BUILD_IDENTIFIER and PROTOCOL_VERSION from the active game config; "
            + PlayPadPreProcessBuild.SetTemplateValuesCommand + " sets the others.",
            Tags = new[] { "elympics/playpad/template" })]
        public static object GetTemplateValues() => CliRun.Run(() => CreateValuesResult("ok", new List<string>()));

        [CliCommand(PlayPadPreProcessBuild.SetTemplateValuesCommand,
            "Set the ElympicsLobby template's ENVIRONMENT, REGION and QUEUE custom values in Player Settings (omitted arguments are left unchanged), "
            + "and report all values like " + PlayPadPreProcessBuild.GetTemplateValuesCommand + ". TEMPLATE_* environment variables still override them in builds.",
            Tags = new[] { "elympics/playpad/template" })]
        public static object SetTemplateValues(
            [CliArg("environment", "Elympics environment the lobby connects to, e.g. prod or stg. Builds default to prod.")] string environment = "",
            [CliArg("region", "Elympics region the lobby plays in (" + PlayPadPreProcessBuild.ListRegionsCommand + " lists the game's regions). WebGL builds with the template fail without one.")]
            string region = "",
            [CliArg("queue", "Matchmaking queue the lobby joins. Builds default to solo.")] string queue = "") => CliRun.Run(() =>
        {
            var requested = new[]
            {
                (PlayPadPreProcessBuild.LobbyEnvironment, environment), (PlayPadPreProcessBuild.Region, region), (PlayPadPreProcessBuild.Queue, queue),
            };
            if (Array.TrueForAll(requested, request => string.IsNullOrEmpty(request.Item2)))
                throw ElympicsCliErrors.InvalidArgumentException("Nothing to set.", "pass at least one of --environment, --region, --queue.");

            var changed = new List<string>();
            foreach (var (value, raw) in requested)
            {
                if (string.IsNullOrEmpty(raw))
                    continue;
                var trimmed = CliArgs.Require(raw, value.Key);
                var key = value.CustomValue;
                if (PlayerSettings.GetTemplateCustomValue(key) == trimmed)
                    continue;
                PlayerSettings.SetTemplateCustomValue(key, trimmed);
                if (PlayerSettings.GetTemplateCustomValue(key) != trimmed)
                    throw new ElympicsCliException(TemplateValueNotSaved, $"Player Settings didn't keep {key} = \"{trimmed}\".",
                        $"select the template first (PlayerSettings.WebGL.template = \"PROJECT:{PlayPadWebGLTemplateInstaller.TemplateName}\"; playpad_get_template_status shows it), then retry.");
                changed.Add(value.Key);
            }
            if (changed.Count > 0)
                AssetDatabase.SaveAssets();
            return CreateValuesResult(changed.Count > 0 ? "updated" : "ok", changed);
        });

        private static object CreateValuesResult(string status, List<string> changed)
        {
            var values = new Dictionary<string, string?>();
            var environmentOverrides = new Dictionary<string, string>();
            foreach (var value in PlayPadPreProcessBuild.TemplateValues)
            {
                values[value.Key] = NullIfEmpty(PlayerSettings.GetTemplateCustomValue(value.CustomValue));
                var overridden = Environment.GetEnvironmentVariable(value.Variable);
                if (!string.IsNullOrEmpty(overridden))
                    environmentOverrides[value.Variable] = overridden;
            }

            return new
            {
                status,
                changed,
                selected = IsTemplateSelected,
                values,
                environmentOverrides,
            };
        }

        private static bool IsNonEmptyDirectory(string path) => Directory.Exists(path) && Directory.EnumerateFileSystemEntries(path).Any();

        private static bool IsTemplateSelected => PlayerSettings.WebGL.template == PlayPadPreProcessBuild.ElympicsTemplateName;

        private static string? NullIfEmpty(string value) => string.IsNullOrEmpty(value) ? null : value;

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
