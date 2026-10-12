#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics.Models.Authentication;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;
using ElympicsPlayPad.ExternalCommunicators.GameStatus;
using ElympicsPlayPad.ExternalCommunicators.Lobby;

namespace ElympicsPlayPad.Session.Strategies
{
    internal class SessionManagerPlatformInitializationStrategy : SessionManagerInitializationStrategy
    {
        private readonly IExternalLobbyCommunicator _lobbyCommunicator;
        private readonly IExternalGameStatusCommunicator _externalGameStatusCommunicator;
        public SessionManagerPlatformInitializationStrategy(IExternalLobbyCommunicator lobbyCommunicator, IExternalGameStatusCommunicator externalGameStatusCommunicator)
        {
            _lobbyCommunicator = lobbyCommunicator;
            _externalGameStatusCommunicator = externalGameStatusCommunicator;
        }

        public override async UniTask InitializePostAuthenticationAsync(
            HandshakeInfo handshake,
            AuthData authData,
            CancellationToken ct)
        {
            _externalGameStatusCommunicator.Dispose();
            _ = await _lobbyCommunicator.GetLobbyStatus(ct);
        }
    }
}
