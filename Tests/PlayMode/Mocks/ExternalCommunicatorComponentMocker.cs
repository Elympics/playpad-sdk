using System;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Elympics.Models.Authentication;
using ElympicsPlayPad.ExternalCommunicators;
using ElympicsPlayPad.ExternalCommunicators.Authentication;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;
using ElympicsPlayPad.ExternalCommunicators.GameStatus;
using ElympicsPlayPad.ExternalCommunicators.GameStatus.Models;
using ElympicsPlayPad.ExternalCommunicators.Tournament;
using ElympicsPlayPad.Tournament.Data;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;

namespace ElympicsPlayPad.Tests.PlayMode
{
    public static class ExternalCommunicatorComponentMocker
    {
        private static readonly IExternalAuthenticator AuthMock = Substitute.For<IExternalAuthenticator>();
        private static readonly IExternalTournamentCommunicator TournamentMock = Substitute.For<IExternalTournamentCommunicator>();
        private static readonly IExternalGameStatusCommunicator GameMock = Substitute.For<IExternalGameStatusCommunicator>();
        private static PlayStatusInfo currentPlayStatus;
        public static PlayPadCommunicator MockInitializationMessage(this PlayPadCommunicator communicator, Capabilities mockCapabilities, FeatureAccess mockFeatureAccess, string environment, string closestRegion)
        {
            _ = AuthMock.InitializationMessage(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(UniTask.FromResult(new HandshakeInfo(false, mockCapabilities, environment, closestRegion, mockFeatureAccess)));
            MockExternalCommunicator(communicator, PlayPadCommunicator.ExternalAuthenticatorFieldName, AuthMock);
            return communicator;
        }

        public static PlayPadCommunicator MockPlayState(this PlayPadCommunicator communicator, PlayStatus statusCode, string label)
        {

            _ = GameMock.CanPlayGame(Arg.Any<bool>()).Returns(x =>
            {
                currentPlayStatus = new PlayStatusInfo()
                {
                    PlayStatus = statusCode,
                    LabelInfo = label,
                };
                return UniTask.FromResult(currentPlayStatus);
            });

            GameMock.CurrentPlayStatus.Returns(currentPlayStatus);

            MockExternalCommunicator(communicator, PlayPadCommunicator.GameStatusCommunicatorFieldName, GameMock);
            return communicator;
        }

        public static PlayPadCommunicator MockAuthentication(
            this PlayPadCommunicator communicator,
            Guid userId,
            string jwt,
            string nickname,
            AuthType authType,
            out IExternalAuthenticator authMock)
        {
            _ = AuthMock.Authenticate().Returns(UniTask.FromResult(new AuthData(userId, jwt, nickname, authType)));
            MockExternalCommunicator(communicator, PlayPadCommunicator.ExternalAuthenticatorFieldName, AuthMock);
            authMock = AuthMock;
            return communicator;
        }

        public static PlayPadCommunicator MockTournament(
            this PlayPadCommunicator communicator,
            string tournamentId,
            int leaderboardCapacity,
            string name,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
        {
            _ = TournamentMock.GetTournament().Returns(UniTask.FromResult<TournamentInfo?>(new TournamentInfo
            {
                Id = tournamentId,
                LeaderboardCapacity = leaderboardCapacity,
                Name = name,
                OwnerId = null,
                StartDate = startDate,
                EndDate = endDate
            }));
            MockExternalCommunicator(communicator, PlayPadCommunicator.TournamentCommunicatorFieldName, TournamentMock);
            return communicator;
        }

        public static void Reset()
        {
            AuthMock.ClearSubstitute();
            TournamentMock.ClearSubstitute();
            GameMock.ClearSubstitute();
        }

        public static void MockExternalWalletCommunicatorAndSet(this PlayPadCommunicator communicator, string address, string chainId)
        {
            // var walletMock = Substitute.For<IExternalWalletCommunicator>();
            // walletMock.Connect().Returns(UniTask.FromResult(new ConnectionResponse()
            // {
            //     address = address,
            //     chainId = chainId,
            // }));
            // communicator.SetCustomExternalWalletCommunicator(walletMock);
        }

        public static void MockExternalWalletCommunicatorWithDisconnectionAndSet(this PlayPadCommunicator communicator, string address, string chainId)
        {
            // var walletMock = Substitute.For<IExternalWalletCommunicator>();
            // IPlayPadEventListener listener = null;
            // walletMock.When(x => x.SetPlayPadEventListener(Arg.Any<IPlayPadEventListener>())).Do(x =>
            // {
            //     listener = x.Arg<IPlayPadEventListener>();
            // });
            //
            // walletMock.Connect().ReturnsForAnyArgs(x => UniTask.FromResult(new ConnectionResponse()
            // {
            //     address = address,
            //     chainId = chainId,
            // })).AndDoes(x => listener.OnWalletDisconnected());
            // communicator.SetCustomExternalWalletCommunicator(walletMock);
        }

        public static void MockExternalWalletCommunicatorWithConnectedAndSet(this PlayPadCommunicator communicator, string address, string chainId)
        {
            // var walletMock = Substitute.For<IExternalWalletCommunicator>();
            // IPlayPadEventListener listener = null;
            // walletMock.When(x => x.SetPlayPadEventListener(Arg.Any<IPlayPadEventListener>())).Do(x =>
            // {
            //     listener = x.Arg<IPlayPadEventListener>();
            // });
            //
            // walletMock.Connect().ReturnsForAnyArgs(x => UniTask.FromResult(new ConnectionResponse()
            // {
            //     address = address,
            //     chainId = chainId,
            // })).AndDoes(x => listener.OnWalletConnected(address, chainId));
            // communicator.SetCustomExternalWalletCommunicator(walletMock);
        }

        private static void MockExternalCommunicator<T>(PlayPadCommunicator playPad, string externalCommunicatorName, T mock)
        {
            var externalCommunicator = playPad.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(x => x.Name == externalCommunicatorName);
            Assert.NotNull(playPad);
            externalCommunicator!.SetValue(playPad, mock);
        }
    }
}
