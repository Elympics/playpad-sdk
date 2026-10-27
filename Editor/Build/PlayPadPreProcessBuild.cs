using System;
using System.Linq;
using Elympics;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ElympicsPlayPad.Editor.Build
{
    public class PlayPadPreProcessBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder => 1000;

        private const string DefaultQueue = "solo";
        private const string DefaultEnvironment = "prod";

        internal const string ElympicsTemplateName = "PROJECT:" + PlayPadWebGLTemplateInstaller.TemplateName;

        internal const string ListRegionsCommand = "elympics_list_regions";
        internal const string GetTemplateValuesCommand = "playpad_get_template_values";
        internal const string SetTemplateValuesCommand = "playpad_set_template_values";

        internal readonly struct TemplateValue
        {
            public TemplateValue(string name)
            {
                Key = name.ToLowerInvariant();
                Variable = "TEMPLATE_" + name;
                CustomValue = "ELYMPICS_" + name;
            }

            /// <summary>CLI name, e.g. <c>region</c>.</summary>
            public string Key { get; }
            public string Variable { get; }
            public string CustomValue { get; }
        }

        internal static readonly TemplateValue GameId = new("GAME_ID");
        internal static readonly TemplateValue GameName = new("GAME_NAME");
        internal static readonly TemplateValue BuildIdentifier = new("BUILD_IDENTIFIER");
        internal static readonly TemplateValue ProtocolVersion = new("PROTOCOL_VERSION");
        internal static readonly TemplateValue Queue = new("QUEUE");
        internal static readonly TemplateValue LobbyEnvironment = new("ENVIRONMENT");
        internal static readonly TemplateValue Region = new("REGION");

        internal static readonly TemplateValue[] TemplateValues = { GameId, GameName, BuildIdentifier, ProtocolVersion, Queue, LobbyEnvironment, Region };

        /// <summary>The template can't start matches without a region, so a build without one fails.</summary>
        internal static string RequireRegion(string fromEnvironment, string fromCustomValue)
        {
            var region = string.IsNullOrWhiteSpace(fromEnvironment) ? fromCustomValue : fromEnvironment;
            if (string.IsNullOrWhiteSpace(region))
                throw new BuildFailedException($"The {PlayPadWebGLTemplateInstaller.TemplateName} WebGL template needs an Elympics region, but none is set. "
                    + $"Set it in Player Settings > Resolution and Presentation ({Region.CustomValue}), with the CLI command {SetTemplateValuesCommand} --region <name>, "
                    + $"or with the {Region.Variable} environment variable ({ListRegionsCommand} lists the game's regions).");
            return region.Trim();
        }

        internal static bool IsSafeTemplateValue(string value) => !value.Any(c => c is '"' or '\\' or '<' or '>' || char.IsControl(c));

        private static string RequireSafe(TemplateValue templateValue, string value) =>
            IsSafeTemplateValue(value)
                ? value
                : throw new BuildFailedException($"The {templateValue.Variable} value \"{value}\" contains a quote, backslash, angle bracket or control character, which the {PlayPadWebGLTemplateInstaller.TemplateName} WebGL template can't embed in index.html. "
                    + $"Remove those characters from it ({templateValue.CustomValue} in Player Settings or the {templateValue.Variable} environment variable).");

        private static string GetEnvOrDefault(string envVarName, string defaultValue)
        {
            var value = Environment.GetEnvironmentVariable(envVarName);
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        private static string GetEnvOrCustomValueOrDefault(TemplateValue value, string defaultValue)
        {
            var resolved = GetEnvOrDefault(value.Variable, PlayerSettings.GetTemplateCustomValue(value.CustomValue));
            return string.IsNullOrWhiteSpace(resolved) ? defaultValue : resolved;
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.WebGL)
                return;

            if (PlayerSettings.WebGL.template != ElympicsTemplateName)
                return;

            PlayPadWebGLTemplateInstaller.UpdateTemplateBeforeBuild();

            Console.WriteLine("Setting Elympics PlayPad WebGL template custom values.");

            var config = ElympicsConfig.LoadCurrentElympicsGameConfig()
                ?? throw new ElympicsException("Elympics config not found");

            var gameId = RequireSafe(GameId, GetEnvOrDefault(GameId.Variable, config.GameId));
            var gameName = RequireSafe(GameName, GetEnvOrDefault(GameName.Variable, config.GameName));
            var buildIdentifier = RequireSafe(BuildIdentifier, GetEnvOrDefault(BuildIdentifier.Variable, config.GameVersion));
            var protocolVersion = RequireSafe(ProtocolVersion, GetEnvOrDefault(ProtocolVersion.Variable, PlayPadMessagingSystem.ProtocolVersion));
            var queue = RequireSafe(Queue, GetEnvOrCustomValueOrDefault(Queue, DefaultQueue));
            var environment = RequireSafe(LobbyEnvironment, GetEnvOrCustomValueOrDefault(LobbyEnvironment, DefaultEnvironment));
            var region = RequireSafe(Region, RequireRegion(Environment.GetEnvironmentVariable(Region.Variable), PlayerSettings.GetTemplateCustomValue(Region.CustomValue)));

            PlayerSettings.SetTemplateCustomValue(GameId.CustomValue, gameId);
            PlayerSettings.SetTemplateCustomValue(GameName.CustomValue, gameName);
            PlayerSettings.SetTemplateCustomValue(BuildIdentifier.CustomValue, buildIdentifier);
            PlayerSettings.SetTemplateCustomValue(ProtocolVersion.CustomValue, protocolVersion);
            PlayerSettings.SetTemplateCustomValue(Queue.CustomValue, queue);
            PlayerSettings.SetTemplateCustomValue(LobbyEnvironment.CustomValue, environment);
            PlayerSettings.SetTemplateCustomValue(Region.CustomValue, region);

            Debug.Log($"Elympics WebGL template values set: GameId={gameId}, "
                      + $"GameName={gameName}, BuildIdentifier={buildIdentifier}, "
                      + $"ProtocolVersion={protocolVersion}, Queue={queue}, Environment={environment}, Region={region}");
        }
    }
}
