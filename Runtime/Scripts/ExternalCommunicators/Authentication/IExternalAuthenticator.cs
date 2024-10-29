using System;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;

namespace ElympicsPlayPad.ExternalCommunicators.Authentication
{
    public interface IExternalAuthenticator : IDisposable
    {
        public UniTask<ExternalAuthData> InitializationMessage(string gameId, string gameName, string versionName, string sdkVersion, string lobbyPackageVersion);
        internal void SetPlayPadEventListener(IPlayPadEventListener listener);
    }
}
