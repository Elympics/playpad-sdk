#nullable enable
#pragma warning disable 67 // AuthenticationUpdated: the guest login never changes.
using System;
using System.Threading;
using Elympics.Cysharp.Threading.Tasks;
using Elympics.Models.Authentication;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;
using UnityEngine;
#if UNITY_EDITOR || !UNITY_WEBGL || ELYMPICS_PLAYPAD_KEEP_LOBBY
using Elympics;
using ElympicsPlayPad.Auth;
using ElympicsPlayPad.Wrappers;
#endif

namespace ElympicsPlayPad.ExternalCommunicators.Authentication
{
    /// <summary>
    /// Stands in for the PlayPad website when the game runs on its own: logs in as a guest with the same client secret as
    /// <see cref="DefaultElympicsLobbyWrapper"/> and starts in lobby mode.
    /// </summary>
    /// <remarks>
    /// Set it as the authenticator in <see cref="Mock.PlayPadMockConfiguration"/>.
    /// Mock communicators work in the Editor, and in players only with ELYMPICS_DISABLE_PLAYPAD.
    /// </remarks>
    public class StandaloneAuthenticationCommunicator : CustomStandaloneAuthenticationCommunicatorBase
    {
        [SerializeField] private Capabilities capabilities = Capabilities.Guest;
        [SerializeField] private FeatureAccess features = ~FeatureAccess.None;
        [Tooltip("Leave empty to let PlayPad find the closest region.")]
        [SerializeField] private string closestRegion = string.Empty;

        public override event Action<string>? RegionUpdated;
        public override event Action<AuthData>? AuthenticationUpdated;

#if UNITY_EDITOR || !UNITY_WEBGL || ELYMPICS_PLAYPAD_KEEP_LOBBY
        internal IAuthClient? AuthClient { get; set; }

        public override async UniTask<AuthData> Authenticate(CancellationToken ct = default)
        {
            var authClient = AuthClient ??= new RemoteAuthClient(ClientSecretAuth.GetAuthUrl(
                ElympicsConfig.Load() ?? throw new InvalidOperationException($"No {nameof(ElympicsConfig)} instance found.")));
            var result = await authClient.AuthenticateWithClientSecret(ClientSecretAuth.GetOrCreateClientSecret(), ct);
            if (result.IsFailure)
                throw new ElympicsException($"Guest authentication failed: {result.Error}");
            return result.Value;
        }
#else
        public override UniTask<AuthData> Authenticate(CancellationToken ct = default) =>
            throw new NotSupportedException("Standalone authentication needs the PlayPad lobby, which WebGL builds keep only with ELYMPICS_PLAYPAD_KEEP_LOBBY.");
#endif

        public override UniTask ChangeRegion(string newRegion, CancellationToken ct = default)
        {
            RegionUpdated?.Invoke(newRegion);
            return UniTask.CompletedTask;
        }

        public override UniTask<HandshakeInfo> InitializationMessage(
            string gameId,
            string gameName,
            string versionName,
            string sdkVersion,
            string lobbyPackageVersion,
            CancellationToken ct = default) =>
            UniTask.FromResult(new HandshakeInfo(
                Application.isMobilePlatform,
                capabilities,
                string.Empty,
                closestRegion,
                features,
                new UserPrefsInfo(Array.Empty<string>()),
                LaunchMode.Lobby | LaunchMode.Gameplay));
    }
}
