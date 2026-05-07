//#define UNITY_6000_3_OR_NEWER

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

        private const string ElympicsTemplateName = "PROJECT:ElympicsPlayPad";

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

#if !UNITY_6000_3_OR_NEWER
            if (EditorUserBuildSettings.development)
                throw new BuildFailedException(
                    "Elympics PlayPad WebGL template does not support Development builds on Unity versions older than 6.3. " + "Please disable Development Build or upgrade to Unity 6.3+.");
#endif
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

            var region = GetEnvOrDefault("TEMPLATE_REGION",
                PlayerSettings.GetTemplateCustomValue("ELYMPICS_REGION"));
            if (!string.IsNullOrEmpty(region))
                PlayerSettings.SetTemplateCustomValue("ELYMPICS_REGION", region);

            Debug.Log($"Elympics WebGL template values set: GameId={gameId}, "
                      + $"GameName={gameName}, BuildIdentifier={buildIdentifier}, "
                      + $"ProtocolVersion={protocolVersion}, Queue={queue}, Environment={environment}, Region={region}");
        }
    }
}
