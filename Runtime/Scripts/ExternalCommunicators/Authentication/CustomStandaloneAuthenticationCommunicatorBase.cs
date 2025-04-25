using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics.Models.Authentication;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Authentication
{
    public abstract class CustomStandaloneAuthenticationCommunicatorBase : MonoBehaviour, IExternalAuthenticator
    {
        public event Action<string> RegionUpdated;
        public abstract event Action<AuthData> AuthenticationUpdated;
        public abstract UniTask<AuthData> Authenticate(CancellationToken ct = default);
        public abstract UniTask ChangeRegion(string newRegion, CancellationToken ct = default);
        public abstract UniTask<HandshakeInfo> InitializationMessage(
            string gameId,
            string gameName,
            string versionName,
            string sdkVersion,
            string lobbyPackageVersion,
            CancellationToken ct = default);
    }
}
