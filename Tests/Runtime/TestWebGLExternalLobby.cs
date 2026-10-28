#nullable enable

using System;
using System.Collections.Generic;
using Elympics;
using Elympics.Cysharp.Threading.Tasks;
using Elympics.Models.Authentication;
using ElympicsPlayPad.ExternalCommunicators.Lobby;
using ElympicsPlayPad.ExternalCommunicators.Lobby.Models;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Responses;
using ElympicsPlayPad.Protocol.WebMessages;
using ElympicsPlayPad.Tests.Lobby.Common.MockSetup;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace ElympicsPlayPad.Tests
{
    [Category("WebLobby")]
    public class TestWebGLExternalLobby
    {
        private const string GameId = "a1b2c3d4-0000-0000-0000-000000000001";
        private static readonly Guid UserId = new("10000000-0000-0000-0000-000000000001");
        private static readonly Guid MatchId = new("20000000-0000-0000-0000-000000000001");
        private static readonly Guid OtherMatchId = new("20000000-0000-0000-0000-000000000002");

        private ElympicsMatchLauncher _launcher = null!;
        private WebGLExternalLobby _sut = null!;
        private List<LobbyInfo> _updates = null!;

        [SetUp]
        protected void SetUp()
        {
            _launcher = Substitute.For<ElympicsMatchLauncher>()
                .SetupLaunch<OnlineLaunchData>(_ => UniTask.FromResult(ElympicsLaunchResult.Succeeded("launch", GameId, MatchId.ToString())));

            var communicatorInternal = new PlayPadCommunicatorInternal(GameId, true, _launcher);
            communicatorInternal.Authenticate(new AuthData(UserId, "jwt", AuthType.ClientSecret), "warsaw", false).Forget();
            _sut = new WebGLExternalLobby(Substitute.For<IPlayPadMessagingSystem>(), communicatorInternal);
            _updates = new List<LobbyInfo>();
            _sut.OnLobbyInfoUpdated += _updates.Add;
        }

        private void ReceiveReadyMatch(Guid matchId) =>
            _sut.OnWebMessage(new WebMessage
            {
                type = WebMessageTypes.LobbyStatusUpdated,
                message = JsonUtility.ToJson(new LobbyStatusResponse
                {
                    matchId = matchId.ToString(),
                    userSecret = "user-secret",
                    queueName = "queue",
                    regionName = "warsaw",
                    gameEngineData = new byte[] { 1 },
                    matchmakerData = new[] { 0.5f },
                    tcpUdpServerAddress = "1.2.3.4:5678",
                    webServerAddress = "https://gs.example/match",
                    matchedPlayers = new[] { UserId.ToString() },
                }),
            });

        private void EndMatch(Guid matchId) =>
            _launcher.MatchEnded += Raise.Event<Action<ElympicsMatchEndedInfo>>(new ElympicsMatchEndedInfo { matchId = matchId.ToString() });

        [Test]
        public void FinishedMatchCannotBePlayedAgain()
        {
            ReceiveReadyMatch(MatchId);
            _sut.PlayMatch();

            EndMatch(MatchId);

            Assert.That(_sut.Lobby.IsMatchReady, Is.False);
            Assert.That(_updates[^1].IsMatchReady, Is.False);
            _ = Assert.Throws<InvalidOperationException>(() => _sut.PlayMatch());
        }

        [Test]
        public void MatchCanBePlayedAgainBeforeItFinishes()
        {
            ReceiveReadyMatch(MatchId);
            _sut.PlayMatch();

            _sut.PlayMatch();

            Assert.That(_sut.Lobby.IsMatchReady, Is.True);
            _launcher.ReceivedLaunch<OnlineLaunchData>(2, GameId, data => data.MatchId == MatchId);
        }

        [Test]
        public void EndOfAnotherMatchKeepsTheLobbyMatch()
        {
            ReceiveReadyMatch(MatchId);
            _sut.PlayMatch();

            EndMatch(OtherMatchId);

            Assert.That(_sut.Lobby.IsMatchReady, Is.True);
        }

        [Test]
        public void NewLobbyMatchCanBePlayedAfterTheLastOneFinished()
        {
            ReceiveReadyMatch(MatchId);
            _sut.PlayMatch();
            EndMatch(MatchId);

            ReceiveReadyMatch(OtherMatchId);
            _sut.PlayMatch();

            _launcher.ReceivedLaunch<OnlineLaunchData>(1, GameId, data => data.MatchId == OtherMatchId);
        }
    }
}
