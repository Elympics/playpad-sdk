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
            Tags = new[] { "elympics/playpad/upload" })]
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

        [CliCommand("playpad_upload_streaming_assets",
            "Upload a directory of StreamingAssets variants to the Elympics cloud as a new content version, with the Editor's current login and "
            + "the active game config unless --game_id is given. Versions are write-once - a burnt version name cannot be reused, and the upload "
            + "is not undoable, so it needs --confirm true. Large bundles take minutes: run it with --detach.",
            Tags = new[] { "elympics/playpad/upload" })]
        public static Task<object> UploadStreamingAssets(
            [CliArg("streaming_assets_path", "Local directory to upload, inside the project unless --allow_external_path true. Absolute, or relative to the project root.", Required = true)] string streamingAssetsPath,
            [CliArg("version_name", "Content version to publish. Write-once - a version name that has already been used (or failed mid-upload) cannot be reused. "
                + "Only Latin letters, digits, dashes, and dots are allowed.", Required = true)]
            string versionName,
            [CliArg("game_id", CliWeb.GameIdDescription)] string gameId = "",
            [CliArg("layout", "Directory layout: AddressableVariants (default, one Addressables build per variant, generating a manifest file) "
                + "or UnstructuredAssets (every file below the path, unvalidated).")]
            string layout = nameof(StreamingAssetsLayout.AddressableVariants),
            [CliArg("confirm", CliWeb.UploadConfirmDescription)] bool confirm = false,
            [CliArg("allow_external_path", "Must be true to upload a directory outside the project.")] bool allowExternalPath = false,
            [CliArg("timeout_seconds", CliWeb.UploadTimeoutDescription)] int timeoutSeconds = CliWeb.DefaultUploadTimeoutSeconds) =>
            CliWeb.RunAsync(timeoutSeconds, async ct =>
            {
                streamingAssetsPath = CliArgs.Require(streamingAssetsPath, "streaming_assets_path");
                versionName = CliArgs.ValidateVersion(versionName, "version_name");
                var parsedLayout = ParseLayout(layout);
                var resolvedPath = CliArgs.ResolveProjectPath(streamingAssetsPath, "streaming_assets_path", allowExternalPath);
                if (!Directory.Exists(resolvedPath))
                    throw new ElympicsCliException(ElympicsCliErrors.BuildDirNotFound, $"The StreamingAssets directory {resolvedPath} doesn't exist.",
                        "build the Addressables content first, then pass its output folder.");
                CliArgs.RequireConfirm(confirm, "Uploading a StreamingAssets version");
                var usedActiveGameConfig = string.IsNullOrWhiteSpace(gameId);
                var resolvedGameId = CliGames.ResolveGameId(gameId);

                int fileCount;
                try
                {
                    fileCount = await ElympicsWebIntegration.UploadStreamingAssetsUsingCurrentLoginAsync(resolvedGameId, resolvedPath, versionName, parsedLayout, timeoutSeconds, ct);
                }
                catch (ElympicsUploadInputException e)
                {
                    throw new ElympicsCliException(ElympicsCliErrors.InvalidArgument, e.Message,
                        "fix the directory layout described above (or pass --layout UnstructuredAssets to upload it as-is), then retry with a new --version_name.");
                }

                return new
                {
                    status = "uploaded",
                    gameId = resolvedGameId,
                    versionName,
                    layout = parsedLayout.ToString(),
                    streamingAssetsPath = resolvedPath,
                    fileCount,
                    usedActiveGameConfig,
                };
            }, ElympicsCliErrors.UploadFailed);

        internal static StreamingAssetsLayout ParseLayout(string layout) =>
            string.IsNullOrWhiteSpace(layout) ? StreamingAssetsLayout.AddressableVariants : CliArgs.ParseEnum<StreamingAssetsLayout>(layout, "layout");
    }
}
