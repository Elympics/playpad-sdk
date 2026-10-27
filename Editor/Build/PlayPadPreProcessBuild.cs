using System;
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

        /// <summary>How <see cref="PlayerSettings.WebGL.template"/> names the template installed by <see cref="PlayPadWebGLTemplateInstaller"/>.</summary>
        internal const string ElympicsTemplateName = "PROJECT:" + PlayPadWebGLTemplateInstaller.TemplateName;

        internal const string RegionVariable = "TEMPLATE_REGION";
        internal const string RegionCustomValue = "ELYMPICS_REGION";

        /// <summary>The ElympicsLobby template can't start matches without a region, so a build without one fails instead of shipping a broken lobby.</summary>
        /// <exception cref="BuildFailedException">Neither the environment variable nor the template custom value sets a region.</exception>
        internal static string RequireRegion(string fromEnvironment, string fromCustomValue)
        {
            var region = string.IsNullOrWhiteSpace(fromEnvironment) ? fromCustomValue : fromEnvironment;
            if (string.IsNullOrWhiteSpace(region))
                throw new BuildFailedException($"The {PlayPadWebGLTemplateInstaller.TemplateName} WebGL template needs an Elympics region, but none is set. "
                    + $"Set it in Player Settings > Resolution and Presentation ({RegionCustomValue}), with the CLI command playpad_template_values --region <name>, "
                    + $"or with the {RegionVariable} environment variable (elympics_list_regions lists the game's regions).");
            return region.Trim();
        }

        private static string GetEnvOrDefault(string envVarName, string defaultValue)
        {
            var value = Environment.GetEnvironmentVariable(envVarName);
            return string.IsNullOrEmpty(value) ? defaultValue : value;
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

            var gameId = GetEnvOrDefault("TEMPLATE_GAME_ID", config.GameId);
            var gameName = GetEnvOrDefault("TEMPLATE_GAME_NAME", config.GameName);
            var buildIdentifier = GetEnvOrDefault("TEMPLATE_BUILD_IDENTIFIER", config.GameVersion);
            var protocolVersion = GetEnvOrDefault("TEMPLATE_PROTOCOL_VERSION", PlayPadMessagingSystem.ProtocolVersion);

            PlayerSettings.SetTemplateCustomValue("ELYMPICS_GAME_ID", gameId);
            PlayerSettings.SetTemplateCustomValue("ELYMPICS_GAME_NAME", gameName);
            PlayerSettings.SetTemplateCustomValue("ELYMPICS_BUILD_IDENTIFIER", buildIdentifier);
            PlayerSettings.SetTemplateCustomValue("ELYMPICS_PROTOCOL_VERSION", protocolVersion);

            var queue = GetEnvOrDefault("TEMPLATE_QUEUE",
                PlayerSettings.GetTemplateCustomValue("ELYMPICS_QUEUE"));
            if (string.IsNullOrEmpty(queue))
                queue = DefaultQueue;
            PlayerSettings.SetTemplateCustomValue("ELYMPICS_QUEUE", queue);

            var environment = GetEnvOrDefault("TEMPLATE_ENVIRONMENT",
                PlayerSettings.GetTemplateCustomValue("ELYMPICS_ENVIRONMENT"));
            if (string.IsNullOrEmpty(environment))
                environment = DefaultEnvironment;
            PlayerSettings.SetTemplateCustomValue("ELYMPICS_ENVIRONMENT", environment);

            var region = RequireRegion(Environment.GetEnvironmentVariable(RegionVariable), PlayerSettings.GetTemplateCustomValue(RegionCustomValue));
            PlayerSettings.SetTemplateCustomValue(RegionCustomValue, region);

            Debug.Log($"Elympics WebGL template values set: GameId={gameId}, "
                      + $"GameName={gameName}, BuildIdentifier={buildIdentifier}, "
                      + $"ProtocolVersion={protocolVersion}, Queue={queue}, Environment={environment}, Region={region}");
        }
    }
}
