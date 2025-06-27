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
        event Action<NewRegionName>? RegionUpdated;
        event Action<AuthData>? AuthenticationUpdated;
        UniTask<AuthData> Authenticate(CancellationToken ct = default);
        UniTask ChangeRegion(string newRegion, CancellationToken ct = default);
        UniTask<HandshakeInfo> InitializationMessage(
            string gameId,
            string gameName,
            string versionName,
            string sdkVersion,
            string lobbyPackageVersion,
            CancellationToken ct = default);
    }
}
