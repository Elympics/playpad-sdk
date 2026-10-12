#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics.Core.Logger;
using Elympics.Models.Authentication;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;
using ElympicsPlayPad.ExternalCommunicators.GameStatus;
using ElympicsPlayPad.Utils;

namespace ElympicsPlayPad.Session.Strategies
{
    internal class SessionManagerPlayPadInitializationStrategy : SessionManagerInitializationStrategy
    {
        private readonly IExternalGameStatusCommunicator _gameStatusCommunicator;

        public SessionManagerPlayPadInitializationStrategy(IExternalGameStatusCommunicator gameStatusCommunicator) =>
            _gameStatusCommunicator = gameStatusCommunicator;

        public override async UniTask InitializePostAuthenticationAsync(
            HandshakeInfo handshake,
            AuthData authData,
            CancellationToken ct)
        {
            var logger = Logger.WithMethodName();

            var (accountWallet, signWallet, _) = WalletAddress.ExtractWalletAddresses(authData);
            ElympicsLogger.State.SetWalletAddress(signWallet ?? accountWallet ?? string.Empty);

            _ = await _gameStatusCommunicator.CanPlayGame(false, ct);

            logger.LogInfo($"PlayPad post-authentication initialization completed.");
        }
    }
}
