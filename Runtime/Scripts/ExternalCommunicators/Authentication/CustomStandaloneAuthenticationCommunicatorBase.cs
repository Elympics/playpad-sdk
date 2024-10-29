using System;
using Cysharp.Threading.Tasks;
using Elympics.Models.Authentication;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Authentication
{
    public abstract class CustomStandaloneAuthenticationCommunicatorBase : MonoBehaviour, IExternalAuthenticator
    {
        public abstract event Action<AuthData> AuthenticationUpdated;
        public abstract UniTask<AuthData> Authenticate();
        public abstract UniTask<HandshakeInfo> InitializationMessage(string gameId, string gameName, string versionName, string sdkVersion, string lobbyPackageVersion);
    }
}
