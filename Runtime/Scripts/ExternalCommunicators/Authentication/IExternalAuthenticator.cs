#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Elympics.Models.Authentication;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;

namespace ElympicsPlayPad.ExternalCommunicators.Authentication
{
    public interface IExternalAuthenticator
    {
        public event Action<AuthData>? AuthenticationUpdated;
        public UniTask<AuthData> Authenticate();
        public UniTask<HandshakeInfo> InitializationMessage(string gameId, string gameName, string versionName, string sdkVersion, string lobbyPackageVersion);
    }
}
