#nullable enable

using System.IO;
using System.Threading.Tasks;
using Elympics;
using Elympics.Editor;
using Elympics.Editor.UnityCli;
using Unity.Pipeline.Commands;

namespace ElympicsPlayPad.Editor.UnityCli
{
    public static class PlayPadUploadCliCommands
    {
        [CliCommand("playpad_upload_client_build",
            "Upload a WebGL client build (its root or Build folder) to the Elympics cloud as a client version linked to a server version. "
            + "Irreversible: needs --confirm true. Large builds take minutes: run it with --detach.",
            Tags = new[] { "elympics", "playpad", "upload" })]
        public static Task<object> UploadClientBuild(
            [CliArg("client_build_path", "The WebGL build's root or its Build folder, inside the project unless --allow_external_path true; absolute or relative to the project.", Required = true)] string clientBuildPath,
            [CliArg("client_game_version", "Client version name: 1-63 Latin letters, digits, dashes and dots.", Required = true)] string clientGameVersion,
            [CliArg("server_game_version", "Server version the client connects to. Defaults to the game config's version.")] string serverGameVersion = "",
            [CliArg("game_id", CliWeb.GameIdDescription)] string gameId = "",
            [CliArg("confirm", CliWeb.UploadConfirmDescription)] bool confirm = false,
            [CliArg("allow_external_path", "Must be true to upload a build outside the project.")] bool allowExternalPath = false,
            [CliArg("timeout_seconds", CliWeb.UploadTimeoutDescription)] int timeoutSeconds = CliWeb.DefaultUploadTimeoutSeconds) =>
            CliWeb.RunAsync(timeoutSeconds, async ct =>
            {
                var resolvedPath = CliArgs.ResolveProjectPath(CliArgs.Require(clientBuildPath, "client_build_path"), "client_build_path", allowExternalPath);
                clientGameVersion = CliArgs.ValidateVersion(clientGameVersion, "client_game_version");
                CliArgs.RequireConfirm(confirm, "Uploading a client build");
                var game = CliGames.ResolveGame(gameId);
                serverGameVersion = string.IsNullOrWhiteSpace(serverGameVersion) ? game.GameVersion : CliArgs.ValidateVersion(serverGameVersion, "server_game_version");
                if (!Directory.Exists(resolvedPath))
                    throw new ElympicsCliException(ElympicsCliErrors.BuildDirNotFound, $"The client build directory {resolvedPath} doesn't exist.",
                        "build the WebGL client first (build --target WebGL), then pass its output folder.");

                int fileCount;
                try
                {
                    fileCount = await ElympicsWebIntegration.UploadClientBuildUsingCurrentLoginAsync(game.GameId, resolvedPath, clientGameVersion, serverGameVersion, timeoutSeconds, ct);
                }
                catch (ElympicsClientBuildFilesException e)
                {
                    throw new ElympicsCliException(ElympicsCliErrors.BuildDirNotFound, e.Message,
                        "pass the root of a complete WebGL build (or its Build folder) with the .data, .wasm, .framework.js, .loader.js and .elympicsmeta.json files.");
                }
                catch (ElympicsUploadInputException e)
                {
                    throw new ElympicsCliException(ElympicsCliErrors.InvalidArgument, e.Message, "fix the argument named in the message, then retry.");
                }
                return new { status = "uploaded", gameId = game.GameId, clientGameVersion, serverGameVersion, fileCount, usedCurrentLogin = true };
            }, ElympicsCliErrors.UploadFailed);
    }
}
