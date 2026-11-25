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
        internal static string? Read()
        {
            var gameMode = new Parameter<string>(envVarName: EnvironmentVariableName, longArgName: "elympics-game-mode");
            var parameters = new Parameter[] { gameMode };
            ParameterList.CollectFromEnvironmentVariables(parameters, Environment.GetEnvironmentVariables());
            ParameterList.CollectFromNamedArguments(parameters, PlayModeArguments.Read());
            ParameterList.CollectFromNamedArguments(parameters, Environment.GetCommandLineArgs());
            gameMode.ValidateAndParseRawValue();
            return gameMode.GetValue();
        }
#else
        internal static string? Read() => null;
#endif
    }
}
