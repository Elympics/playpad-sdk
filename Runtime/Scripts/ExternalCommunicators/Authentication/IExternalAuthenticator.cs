#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics.Models.Authentication;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;
using NewRegionName = System.String;

namespace ElympicsPlayPad.ExternalCommunicators.Authentication
{
    public interface IExternalAuthenticator
    {
        public event Action<NewRegionName>? RegionUpdated;
        public event Action<AuthData>? AuthenticationUpdated;
        public UniTask<AuthData> Authenticate(CancellationToken ct = default);
        public UniTask ChangeRegion(string newRegion, CancellationToken ct = default);
        public UniTask<HandshakeInfo> InitializationMessage(
            string gameId,
            string gameName,
            string versionName,
            string sdkVersion,
            string lobbyPackageVersion,
            CancellationToken ct = default);
    }
}
