#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics.Models.Authentication;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;
using ElympicsPlayPad.Protocol.Responses;

namespace ElympicsPlayPad.ExternalCommunicators.Authentication
{
    public class StandaloneExternalAuthenticator : IExternalAuthenticator
    {
        public event Action<string>? RegionUpdated;
        public event Action<AuthData>? AuthenticationUpdated;
        public StandaloneExternalAuthenticator(StandaloneExternalAuthenticatorConfig authConfig) => _authConfig = authConfig;

        private readonly StandaloneExternalAuthenticatorConfig _authConfig;
        public UniTask<AuthData> Authenticate(CancellationToken ct = default) => UniTask.FromResult(new AuthData(Guid.NewGuid(), string.Empty, "StandaloneResult", AuthType.ClientSecret));
        public async UniTask<HandshakeInfo> InitializationMessage(
            string gameId,
            string gameName,
            string versionName,
            string sdkVersion,
            string lobbyPackageVersion,
            CancellationToken ct = default)
        {
            var result = await UniTask.FromResult(new HandshakeResponse
            {
                device = "desktop",
                capabilities = (int)_authConfig.Capabilities,
                closestRegion = _authConfig.ClosestRegion
            });
            return new HandshakeInfo(result.device == "Mobile", (Capabilities)result.capabilities, result.environment, result.closestRegion, _authConfig.FeatureAccess);
        }
    }
}
