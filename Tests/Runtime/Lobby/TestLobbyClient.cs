#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.AssemblyCommunicator;
using Elympics.AssemblyCommunicator.Events;
using Elympics.Models.Authentication;
using Elympics.SnapshotAnalysis.Retrievers;
using ElympicsPlayPad.Auth;
using ElympicsPlayPad.Auth.Models;
using ElympicsPlayPad.Lobby;
using ElympicsPlayPad.Rooms;
using ElympicsPlayPad.Rooms.Models;
using ElympicsPlayPad.Tests.Lobby.Common;
using ElympicsPlayPad.Tests.Lobby.Common.MockSetup;
using ElympicsPlayPad.Wrappers;
using HybridWebSocket;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ElympicsPlayPad.Tests.Lobby
{
    [Category("LobbyClient")]
    public class TestLobbyClient : IElympicsObserver<ElympicsStateChanged>
    {
        private const string Region = "warsaw";
        private const string Nickname = "Nickname";
        private const string ExpiredToken = "eyJhbGciOiJub25lIn0.eyJleHAiOjF9."; // none-alg token with exp set to 1970
        private static readonly Guid UserId = new("10000000-0000-0000-0000-000000000001");
        private static readonly Guid MatchId = new("20000000-0000-0000-0000-000000000001");

        private IAuthClient _auth = null!;
        private ElympicsMatchLauncher _launcher = null!;
        private IWebSocket _ws = null!;
        private ElympicsGameConfig _game = null!;
        private LobbyClient _sut = null!;
        private readonly List<ElympicsState> _states = new();
        private readonly List<ElympicsConnectionData> _established = new();
        private readonly List<ElympicsConnectionLostData> _lost = new();
        private readonly List<SnapshotAnalysisRetriever> _replays = new();
        private UniTaskCompletionSource<ElympicsLaunchResult> _launchResult = null!;

        public void OnEvent(ElympicsStateChanged argument) => _states.Add(argument.NewState);

        [OneTimeSetUp]
        protected void Observe() => CrossAssemblyEventBroadcaster.AddObserver(this);

        [SetUp]
        protected void SetUp()
        {
            _states.Clear();
            _established.Clear();
            _lost.Clear();
            _replays.Clear();
            _launchResult = new UniTaskCompletionSource<ElympicsLaunchResult>();

            _game = ScriptableObject.CreateInstance<ElympicsGameConfig>();
            _game.gameId = Guid.NewGuid().ToString();
            JsonUtility.FromJsonOverwrite("{\"useWeb\":true}", _game);
            _auth = Substitute.For<IAuthClient>().CreateSuccessIAuthClient(UserId);
            var regions = Substitute.For<IAvailableRegionRetriever>();
            _ = regions.GetAvailableRegions().Returns(_ => UniTask.FromResult(new List<string> { Region }));
            _launcher = Substitute.For<ElympicsMatchLauncher>();
            _ = _launcher.State.Returns(MatchLauncherState.Idle);
            _ = _launcher.LaunchOnlineAsync(Arg.Any<string>(), Arg.Any<OnlineLaunchData>(), Arg.Any<ElympicsLaunchOptions?>(), Arg.Any<CancellationToken>())
                .Returns(_ => _launchResult.Task);
            _ws = Substitute.For<IWebSocket>()
                .SetupOpenCloseDefaultBehaviour()
                .SetupJoinLobby(false, UserId, Nickname, string.Empty)
                .SetShowAuthMessage(UserId, Nickname, string.Empty);
            var dispatcher = AsyncEventsDispatcherMockSetup.CreateMockAsyncEventsDispatcher();

            _ = _launcher.LaunchSnapshotReplayAsync(Arg.Any<string>(), Arg.Any<SnapshotReplayLaunchData>(), Arg.Any<ElympicsLaunchOptions?>(), Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    _replays.Add(call.ArgAt<SnapshotReplayLaunchData>(1).Preloaded!);
                    return _launchResult.Task;
                });

            _sut = new LobbyClient(_auth, "client-secret", regions, _launcher,
                _game, "wss://lobby.example/lobby",
                controller => new WebSocketSession(controller, dispatcher, LobbyTestSerialization.Options, (_, _) => _ws));
            _sut.ElympicsConnectionEstablished += _established.Add;
            _sut.ElympicsConnectionLost += _lost.Add;
        }

        [TearDown]
        protected void TearDown()
        {
            LogAssert.ignoreFailingMessages = false;
            _sut.Dispose();
            UnityEngine.Object.DestroyImmediate(_game);
        }

        private static ConnectionData ClientSecretLogin => new() { AuthType = AuthType.ClientSecret, Region = new RegionData(Region) };

        private UniTask Connect() => _sut.ConnectToElympicsAsync(ClientSecretLogin);

        private void SetLauncherIdle() => _launcher.StateChanged += Raise.Event<Action<MatchLauncherState>>(MatchLauncherState.Idle);

        private static RoomMatchData Match() => new(MatchId,
            new MatchDetails(new[] { UserId }, "1.2.3.4:5678", "https://gs.example/match", "user-secret", Array.Empty<byte>(), Array.Empty<float>()),
            "queue",
            Region);

        #region Connection

        [UnityTest]
        public IEnumerator ClientSecretLoginConnects() => UniTask.ToCoroutine(async () =>
        {
            await Connect();

            Assert.That(_sut.State, Is.EqualTo(ElympicsState.Connected));
            Assert.That(_sut.AuthData?.UserId, Is.EqualTo(UserId));
            Assert.That(_sut.ElympicsUser?.Nickname, Is.EqualTo(Nickname));
            Assert.That(_sut.CurrentRegion, Is.EqualTo(Region));
            Assert.That(_sut.AvailableRegions, Is.EqualTo(new[] { Region }));
            Assert.That(_sut.WebSocketSession.IsConnected, Is.True);
            Assert.That(_states, Is.EqualTo(new[] { ElympicsState.Connecting, ElympicsState.Connected }));
            Assert.That(_established, Has.Count.EqualTo(1));
            Assert.That(_established[0].AutoReconnected, Is.False);
            _ = _auth.Received(1).AuthenticateWithClientSecret("client-secret", Arg.Any<CancellationToken>());
        });

        [UnityTest]
        public IEnumerator CachedLoginConnectsWithoutTheAuthService() => UniTask.ToCoroutine(async () =>
        {
            var cached = new AuthData(UserId, AuthClientMockSetup.NonExpiringToken, AuthType.ClientSecret);

            await _sut.ConnectToElympicsAsync(new ConnectionData { Region = new RegionData(Region), AuthFromCacheData = new CachedAuthData(cached, false) });

            Assert.That(_sut.State, Is.EqualTo(ElympicsState.Connected));
            Assert.That(_sut.AuthData, Is.SameAs(cached));
            _ = _auth.DidNotReceive().AuthenticateWithClientSecret(Arg.Any<string>(), Arg.Any<CancellationToken>());
        });

        [UnityTest]
        public IEnumerator ExpiredCachedLoginWithAutoRetryUsesTheAuthService() => UniTask.ToCoroutine(async () =>
        {
            var cached = new AuthData(UserId, ExpiredToken, AuthType.ClientSecret);

            await _sut.ConnectToElympicsAsync(new ConnectionData { Region = new RegionData(Region), AuthFromCacheData = new CachedAuthData(cached, true) });

            Assert.That(_sut.State, Is.EqualTo(ElympicsState.Connected));
            Assert.That(_sut.AuthData, Is.Not.SameAs(cached));
            _ = _auth.Received(1).AuthenticateWithClientSecret(Arg.Any<string>(), Arg.Any<CancellationToken>());
        });

        [UnityTest]
        public IEnumerator ExpiredCachedLoginWithoutAutoRetryFails() => UniTask.ToCoroutine(async () =>
        {
            var cached = new AuthData(UserId, ExpiredToken, AuthType.ClientSecret);
            LogAssert.ignoreFailingMessages = true;

            var connect = _sut.ConnectToElympicsAsync(new ConnectionData { Region = new RegionData(Region), AuthFromCacheData = new CachedAuthData(cached, false) });

            _ = await AsyncAsserts.AssertThrowsAsync<ElympicsException>(connect);
            Assert.That(_sut.State, Is.EqualTo(ElympicsState.Disconnected));
            Assert.That(_sut.AuthData, Is.Null);
            Assert.That(_states, Is.EqualTo(new[] { ElympicsState.Connecting, ElympicsState.Disconnected }));
        });

        [UnityTest]
        public IEnumerator FailedAuthenticationLeavesTheClientDisconnected() => UniTask.ToCoroutine(async () =>
        {
            _ = _auth.CreateFailureIAuthClient();
            LogAssert.ignoreFailingMessages = true;

            _ = await AsyncAsserts.AssertThrowsAsync<ElympicsException>(Connect());

            Assert.That(_sut.State, Is.EqualTo(ElympicsState.Disconnected));
            Assert.That(_sut.IsAuthenticated, Is.False);
            Assert.That(_established, Is.Empty);
        });

        [UnityTest]
        public IEnumerator SignOutDisconnects() => UniTask.ToCoroutine(async () =>
        {
            await Connect();

            _sut.SignOut();

            Assert.That(_sut.State, Is.EqualTo(ElympicsState.Disconnected));
            Assert.That(_sut.AuthData, Is.Null);
            Assert.That(_sut.ElympicsUser, Is.Null);
            Assert.That(_sut.WebSocketSession.IsConnected, Is.False);
        });

        [Test]
        public void SignOutWhileDisconnectedOnlyWarns()
        {
            _sut.SignOut();

            Assert.That(_sut.State, Is.EqualTo(ElympicsState.Disconnected));
            Assert.That(_states, Is.Empty);
        }

        [UnityTest]
        public IEnumerator TimeoutReconnects() => UniTask.ToCoroutine(async () =>
        {
            await Connect();
            _sut.WebSocketSession.Disconnect(DisconnectionReason.Timeout);

            await _sut.ReconnectIfPossible(new DisconnectionData(DisconnectionReason.Timeout));

            Assert.That(_sut.State, Is.EqualTo(ElympicsState.Connected));
            Assert.That(_sut.WebSocketSession.IsConnected, Is.True);
            Assert.That(_states, Is.EqualTo(new[] { ElympicsState.Connecting, ElympicsState.Connected, ElympicsState.Reconnecting, ElympicsState.Connected }));
            Assert.That(_established, Has.Count.EqualTo(2));
            Assert.That(_established[1].AutoReconnected, Is.True);
            Assert.That(_lost, Is.Empty);
        });

        [UnityTest]
        public IEnumerator OtherDisconnectionsAreReportedAsLost() => UniTask.ToCoroutine(async () =>
        {
            await Connect();
            _sut.WebSocketSession.Disconnect(DisconnectionReason.Closed);

            await _sut.ReconnectIfPossible(new DisconnectionData(DisconnectionReason.Closed));

            Assert.That(_sut.State, Is.EqualTo(ElympicsState.Disconnected));
            Assert.That(_lost, Has.Count.EqualTo(1));
            Assert.That(_lost[0].DisconnectionData.Reason, Is.EqualTo(DisconnectionReason.Closed));
        });

        #endregion

        #region Launches

        [UnityTest]
        public IEnumerator RoomsMatchIsPlayingUntilTheLauncherIsIdle() => UniTask.ToCoroutine(async () =>
        {
            await Connect();

            ((IMatchLauncher)_sut).PlayMatch(Match());

            Assert.That(_sut.State, Is.EqualTo(ElympicsState.PlayingMatch));
            _ = _launcher.Received(1).LaunchOnlineAsync(_game.GameId,
                Arg.Is<OnlineLaunchData>(data => data.MatchId == MatchId && data.UserId == UserId && data.ServerAddress == "https://gs.example/match"),
                Arg.Any<ElympicsLaunchOptions?>(), Arg.Any<CancellationToken>());

            _ = _launchResult.TrySetResult(ElympicsLaunchResult.Succeeded("launch", _game.GameId, MatchId.ToString()));
            await UniTask.Yield();
            Assert.That(_sut.State, Is.EqualTo(ElympicsState.PlayingMatch));

            SetLauncherIdle();

            Assert.That(_sut.State, Is.EqualTo(ElympicsState.Connected));
            Assert.That(_states, Is.EqualTo(new[] { ElympicsState.Connecting, ElympicsState.Connected, ElympicsState.PlayingMatch, ElympicsState.Connected }));
        });

        [UnityTest]
        public IEnumerator RejectedLaunchReturnsToConnected() => UniTask.ToCoroutine(async () =>
        {
            await Connect();
            LogAssert.ignoreFailingMessages = true;

            ((IMatchLauncher)_sut).PlayMatch(Match());
            _ = _launchResult.TrySetResult(ElympicsLaunchResult.Failed("launch", ElympicsLaunchErrorCode.InvalidPayload, "invalid"));
            await UniTask.Yield();

            Assert.That(_sut.State, Is.EqualTo(ElympicsState.Connected));
        });

        [UnityTest]
        public IEnumerator SocketLossDuringAMatchIsAppliedAtIdle() => UniTask.ToCoroutine(async () =>
        {
            await Connect();
            ((IMatchLauncher)_sut).PlayMatch(Match());
            _sut.WebSocketSession.Disconnect(DisconnectionReason.Closed);

            await _sut.ReconnectIfPossible(new DisconnectionData(DisconnectionReason.Closed));

            Assert.That(_sut.State, Is.EqualTo(ElympicsState.PlayingMatch));
            Assert.That(_lost, Has.Count.EqualTo(1));

            SetLauncherIdle();

            Assert.That(_sut.State, Is.EqualTo(ElympicsState.Disconnected));
            Assert.That(_states, Is.EqualTo(new[] { ElympicsState.Connecting, ElympicsState.Connected, ElympicsState.PlayingMatch, ElympicsState.Disconnected }));
        });

        [UnityTest]
        public IEnumerator ConnectingDuringAMatchIsRefused() => UniTask.ToCoroutine(async () =>
        {
            await Connect();
            ((IMatchLauncher)_sut).PlayMatch(Match());
            LogAssert.ignoreFailingMessages = true;

            _ = await AsyncAsserts.AssertThrowsAsync<ElympicsException>(Connect());
            _ = Assert.Throws<ElympicsException>(() => _sut.SignOut());
            Assert.That(_sut.State, Is.EqualTo(ElympicsState.PlayingMatch));
        });

        [UnityTest]
        public IEnumerator MatchmakingDuringAMatchIsRefused() => UniTask.ToCoroutine(async () =>
        {
            await Connect();
            ((IMatchLauncher)_sut).PlayMatch(Match());
            var room = Substitute.For<IRoom>();

            _ = await AsyncAsserts.AssertThrowsAsync<ElympicsException>(((IMatchLauncher)_sut).StartMatchmaking(room));

            _ = room.DidNotReceive().StartMatchmakingInternal();
            Assert.That(_sut.State, Is.EqualTo(ElympicsState.PlayingMatch));
        });

        [UnityTest]
        public IEnumerator RoomsMatchIsRefusedWhileTheLauncherIsUnloading() => UniTask.ToCoroutine(async () =>
        {
            await Connect();
            _ = _launcher.State.Returns(MatchLauncherState.Unloading);

            _ = Assert.Throws<InvalidOperationException>(() => ((IMatchLauncher)_sut).PlayMatch(Match()));

            Assert.That(_sut.State, Is.EqualTo(ElympicsState.Connected));
            _ = _launcher.DidNotReceive().LaunchOnlineAsync(Arg.Any<string>(), Arg.Any<OnlineLaunchData>(), Arg.Any<ElympicsLaunchOptions?>(), Arg.Any<CancellationToken>());
        });

        [Test]
        public void ReplayIsWatchedWithoutALobbyConnection()
        {
            var retriever = Substitute.For<SnapshotAnalysisRetriever>();

            _sut.WatchReplay(retriever);

            Assert.That(_sut.State, Is.EqualTo(ElympicsState.WatchReplay));
            Assert.That(_replays, Is.EqualTo(new[] { retriever }));

            SetLauncherIdle();

            Assert.That(_sut.State, Is.EqualTo(ElympicsState.Disconnected));
            Assert.That(_states, Is.EqualTo(new[] { ElympicsState.WatchReplay, ElympicsState.Disconnected }));
        }

        [Test]
        public void MatchesStartedByOtherCallersAreNotReported()
        {
            _launcher.StateChanged += Raise.Event<Action<MatchLauncherState>>(MatchLauncherState.Loading);
            _launcher.StateChanged += Raise.Event<Action<MatchLauncherState>>(MatchLauncherState.Running);
            SetLauncherIdle();

            Assert.That(_states, Is.Empty);
        }

        #endregion
    }
}
