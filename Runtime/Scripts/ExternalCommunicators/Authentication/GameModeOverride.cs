#nullable enable
#if UNITY_EDITOR || !UNITY_WEBGL
using System;
using Elympics;
#endif

namespace ElympicsPlayPad.ExternalCommunicators.Authentication
{
    /// <summary>
    /// The <c>ELYMPICS_GAME_MODE</c> launch parameter (<c>--elympics-game-mode</c>), which replaces the host's game mode in the Editor and non-WebGL players.
    /// </summary>
    internal static class GameModeOverride
    {
        internal const string EnvironmentVariableName = "ELYMPICS_GAME_MODE";

#if UNITY_EDITOR || !UNITY_WEBGL
        private static readonly Parameter<string> Parameter = new(EnvironmentVariableName, "elympicsGameMode", "elympics-game-mode");

        internal static string? Read()
        {
            Parameter.Reset();
            var parameters = new Parameter[] { Parameter };
            ParameterList.CollectFromEnvironmentVariables(parameters, Environment.GetEnvironmentVariables());
            ParameterList.CollectFromNamedArguments(parameters, PlayModeArguments.Read());
            ParameterList.CollectFromNamedArguments(parameters, Environment.GetCommandLineArgs());
            Parameter.ValidateAndParseRawValue();
            return Parameter.GetValue();
        }
#else
        internal static string? Read() => null;
#endif
    }
}
