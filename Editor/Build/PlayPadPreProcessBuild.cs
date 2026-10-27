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

        internal const string ListRegionsCommand = "elympics_list_regions";
        internal const string GetTemplateValuesCommand = "playpad_get_template_values";

        /// <summary>One value of the ElympicsLobby template: a <c>TEMPLATE_*</c> environment variable (which wins) and an <c>ELYMPICS_*</c> template custom value.</summary>
        internal readonly struct TemplateValue
        {
            public TemplateValue(string name)
            {
                Name = name;
                Variable = "TEMPLATE_" + name;
                CustomValue = "ELYMPICS_" + name;
            }

            public string Name { get; }
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

        /// <summary>The ElympicsLobby template can't start matches without a region, so a build without one fails instead of shipping a broken lobby.</summary>
        /// <exception cref="BuildFailedException">Neither the environment variable nor the template custom value sets a region.</exception>
        internal static string RequireRegion(string fromEnvironment, string fromCustomValue)
        {
            var region = string.IsNullOrWhiteSpace(fromEnvironment) ? fromCustomValue : fromEnvironment;
            if (string.IsNullOrWhiteSpace(region))
                throw new BuildFailedException($"The {PlayPadWebGLTemplateInstaller.TemplateName} WebGL template needs an Elympics region, but none is set. "
                    + $"Set it in Player Settings > Resolution and Presentation ({Region.CustomValue}), with the CLI command {GetTemplateValuesCommand} --region <name>, "
                    + $"or with the {Region.Variable} environment variable ({ListRegionsCommand} lists the game's regions).");
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

            var gameId = GetEnvOrDefault(GameId.Variable, config.GameId);
            var gameName = GetEnvOrDefault(GameName.Variable, config.GameName);
            var buildIdentifier = GetEnvOrDefault(BuildIdentifier.Variable, config.GameVersion);
            var protocolVersion = GetEnvOrDefault(ProtocolVersion.Variable, PlayPadMessagingSystem.ProtocolVersion);

            PlayerSettings.SetTemplateCustomValue(GameId.CustomValue, gameId);
            PlayerSettings.SetTemplateCustomValue(GameName.CustomValue, gameName);
            PlayerSettings.SetTemplateCustomValue(BuildIdentifier.CustomValue, buildIdentifier);
            PlayerSettings.SetTemplateCustomValue(ProtocolVersion.CustomValue, protocolVersion);

            var queue = GetEnvOrDefault(Queue.Variable,
                PlayerSettings.GetTemplateCustomValue(Queue.CustomValue));
            if (string.IsNullOrEmpty(queue))
                queue = DefaultQueue;
            PlayerSettings.SetTemplateCustomValue(Queue.CustomValue, queue);

            var environment = GetEnvOrDefault(LobbyEnvironment.Variable,
                PlayerSettings.GetTemplateCustomValue(LobbyEnvironment.CustomValue));
            if (string.IsNullOrEmpty(environment))
                environment = DefaultEnvironment;
            PlayerSettings.SetTemplateCustomValue(LobbyEnvironment.CustomValue, environment);

            var region = RequireRegion(Environment.GetEnvironmentVariable(Region.Variable), PlayerSettings.GetTemplateCustomValue(Region.CustomValue));
            PlayerSettings.SetTemplateCustomValue(Region.CustomValue, region);

            Debug.Log($"Elympics WebGL template values set: GameId={gameId}, "
                      + $"GameName={gameName}, BuildIdentifier={buildIdentifier}, "
                      + $"ProtocolVersion={protocolVersion}, Queue={queue}, Environment={environment}, Region={region}");
        }
    }
}
