#nullable enable

using System;
using System.Collections;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.Models.Authentication;
using ElympicsPlayPad.Rooms;
using ElympicsPlayPad.Rooms.Models;
using ElympicsPlayPad.Wrappers;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace ElympicsPlayPad.Tests
{
    [Category("WebLobby")]
    public class TestPlayPadCommunicatorInternal
    {
        private const string GameId = "a1b2c3d4-0000-0000-0000-000000000001";
        private const string TcpUdpAddress = "1.2.3.4:5678";
        private const string WebAddress = "https://gs.example/match";
        private static readonly Guid UserId = new("10000000-0000-0000-0000-000000000001");
        private static readonly Guid OtherPlayer = new("10000000-0000-0000-0000-000000000002");
        private static readonly Guid MatchId = new("20000000-0000-0000-0000-000000000001");

        private IElympicsMatchLauncher _launcher = null!;
        private OnlineLaunchData? _onlineData;
        private SinglePlayerLaunchData? _singlePlayerData;

        [SetUp]
        public void SetUp()
        {
            _onlineData = null;
            _singlePlayerData = null;
            _launcher = Substitute.For<IElympicsMatchLauncher>();
            _ = _launcher.LaunchOnlineAsync(Arg.Any<string>(), Arg.Do<OnlineLaunchData>(data => _onlineData = data), Arg.Any<ElympicsLaunchOptions?>(), Arg.Any<CancellationToken>())
                .Returns(UniTask.FromResult(ElympicsLaunchResult.Succeeded("launch", GameId, MatchId.ToString())));
            _ = _launcher.LaunchSinglePlayerAsync(Arg.Any<string>(), Arg.Do<SinglePlayerLaunchData?>(data => _singlePlayerData = data), Arg.Any<ElympicsLaunchOptions?>(), Arg.Any<CancellationToken>())
                .Returns(UniTask.FromResult(ElympicsLaunchResult.Succeeded("launch", GameId, MatchId.ToString())));
            LaunchedMatch.Clear();
        }

        private PlayPadCommunicatorInternal CreateAuthenticated(bool useWeb)
        {
            var sut = new PlayPadCommunicatorInternal(GameId, useWeb, () => _launcher);
            sut.Authenticate(new AuthData(UserId, "jwt", AuthType.ClientSecret), "warsaw", false).Forget();
            return sut;
        }

        private static RoomMatchData Match(string tcpUdpAddress = TcpUdpAddress, string webAddress = WebAddress) => new(MatchId,
            new MatchDetails(new[] { UserId, OtherPlayer }, tcpUdpAddress, webAddress, "user-secret", new byte[] { 1, 2 }, new[] { 0.5f }),
            "queue",
            "warsaw");

        [UnityTest]
        public IEnumerator WebRtcGameLaunchesOnlineWithTheWebAddress() => UniTask.ToCoroutine(async () =>
        {
            var result = await CreateAuthenticated(useWeb: true).PlayMatchInternal(Match());

            Assert.That(result.Success, Is.True);
            Assert.That(_onlineData, Is.Not.Null);
            Assert.That(_onlineData!.ServerAddress, Is.EqualTo(WebAddress));
            Assert.That(_onlineData.UseTcpUdpInsteadOfWebRtc, Is.False);
            Assert.That(_onlineData.MatchId, Is.EqualTo(MatchId));
            Assert.That(_onlineData.UserSecret, Is.EqualTo("user-secret"));
            Assert.That(_onlineData.UserId, Is.EqualTo(UserId));
            Assert.That(_onlineData.MatchedPlayers, Is.EqualTo(new[] { UserId, OtherPlayer }));
            Assert.That(_onlineData.RegionName, Is.EqualTo("warsaw"));
            Assert.That(_onlineData.GameEngineData, Is.EqualTo(new byte[] { 1, 2 }));
            Assert.That(_onlineData.MatchmakerData, Is.EqualTo(new[] { 0.5f }));
            _ = _launcher.Received(1).LaunchOnlineAsync(GameId, Arg.Any<OnlineLaunchData>(), Arg.Any<ElympicsLaunchOptions?>(), Arg.Any<CancellationToken>());
        });

        [UnityTest]
        public IEnumerator TcpUdpGameLaunchesOnlineWithTheTcpUdpAddress() => UniTask.ToCoroutine(async () =>
        {
            _ = await CreateAuthenticated(useWeb: false).PlayMatchInternal(Match());

            Assert.That(_onlineData, Is.Not.Null);
            Assert.That(_onlineData!.ServerAddress, Is.EqualTo(TcpUdpAddress));
            Assert.That(_onlineData.UseTcpUdpInsteadOfWebRtc, Is.True);
        });

        [UnityTest]
        public IEnumerator MatchWithoutAddressesLaunchesSinglePlayer() => UniTask.ToCoroutine(async () =>
        {
            _ = await CreateAuthenticated(useWeb: true).PlayMatchInternal(Match(string.Empty, string.Empty));

            Assert.That(_onlineData, Is.Null);
            Assert.That(_singlePlayerData, Is.Not.Null);
            Assert.That(_singlePlayerData!.MatchId, Is.EqualTo(MatchId));
            Assert.That(_singlePlayerData.UserId, Is.EqualTo(UserId));
            Assert.That(_singlePlayerData.QueueName, Is.EqualTo("queue"));
        });

        [UnityTest]
        public IEnumerator LaunchRecordsTheMatchForPlayPadReports() => UniTask.ToCoroutine(async () =>
        {
            _ = await CreateAuthenticated(useWeb: true).PlayMatchInternal(Match());

            Assert.That(LaunchedMatch.MatchId, Is.EqualTo(MatchId.ToString()));
            Assert.That(LaunchedMatch.UserId, Is.EqualTo(UserId));
        });

        [UnityTest]
        public IEnumerator RejectedLaunchForgetsTheMatch() => UniTask.ToCoroutine(async () =>
        {
            _ = _launcher.LaunchOnlineAsync(Arg.Any<string>(), Arg.Any<OnlineLaunchData>(), Arg.Any<ElympicsLaunchOptions?>(), Arg.Any<CancellationToken>())
                .Returns(UniTask.FromResult(ElympicsLaunchResult.Failed("launch", ElympicsLaunchErrorCode.InvalidPayload, "invalid")));

            var result = await CreateAuthenticated(useWeb: true).PlayMatchInternal(Match());

            Assert.That(result.Success, Is.False);
            Assert.That(LaunchedMatch.MatchId, Is.Empty);
        });

        [Test]
        public void PlayingBeforeAuthenticationThrows()
        {
            var sut = new PlayPadCommunicatorInternal(GameId, true, () => _launcher);

            _ = Assert.Throws<InvalidOperationException>(() => sut.PlayMatchInternal(Match()));
            Assert.That(_launcher.ReceivedCalls().Any(), Is.False);
        }
    }
}
