#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics.Core.Logger;
using Elympics.Models.Authentication;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Extensions;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;
using ElympicsPlayPad.ExternalCommunicators.GameStatus;
using ElympicsPlayPad.ExternalCommunicators.Leaderboard;
using ElympicsPlayPad.ExternalCommunicators.Tournament;
using ElympicsPlayPad.ExternalCommunicators.VirtualDeposit;
using ElympicsPlayPad.Utils;

namespace ElympicsPlayPad.Session.Strategies
{
    internal class SessionManagerPlayPadInitializationStrategy : SessionManagerInitializationStrategy
    {
        private readonly IExternalGameStatusCommunicator _gameStatusCommunicator;
        private readonly IExternalTournamentCommunicator _tournamentCommunicator;
        private readonly IExternalLeaderboardCommunicator _leaderboardCommunicator;
        private readonly IExternalCoinCommunicator? _coinCommunicator;

        public SessionManagerPlayPadInitializationStrategy(
            IExternalGameStatusCommunicator gameStatusCommunicator,
            IExternalTournamentCommunicator tournamentCommunicator,
            IExternalLeaderboardCommunicator leaderboardCommunicator,
            IExternalCoinCommunicator? coinCommunicator)
        {
            _gameStatusCommunicator = gameStatusCommunicator;
            _tournamentCommunicator = tournamentCommunicator;
            _leaderboardCommunicator = leaderboardCommunicator;
            _coinCommunicator = coinCommunicator;
        }

        public override async UniTask InitializePostAuthenticationAsync(
            HandshakeInfo handshake,
            AuthData authData,
            CancellationToken ct)
        {
            var logger = Logger.WithMethodName();

            var (accountWallet, signWallet, _) = WalletAddress.ExtractWalletAddresses(authData);
            ElympicsLogger.State.SetWalletAddress(signWallet ?? accountWallet ?? string.Empty);

            if (handshake.FeatureAccess.HasTournament())
            {
                var tournament = await _tournamentCommunicator.GetTournament(ct);
                ElympicsLogger.State.SetTournamentId(tournament?.Id);
            }

            _ = await _gameStatusCommunicator.CanPlayGame(false, ct);

            if (_coinCommunicator != null)
                _ = await _coinCommunicator.GetElympicsCoins(ct);

            if (handshake.FeatureAccess.HasLeaderboard())
                _ = await _leaderboardCommunicator.FetchLeaderboard(ct);

            if (handshake.FeatureAccess.HasUserHighScore())
                _ = await _leaderboardCommunicator.FetchUserHighScore(ct);

            logger.LogInfo($"PlayPad post-authentication initialization completed.");
        }
    }
}
