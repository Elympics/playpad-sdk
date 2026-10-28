using System;
using System.Collections;
using System.Threading;
using Elympics;
using Elympics.Cysharp.Threading.Tasks;
using ElympicsPlayPad.Auth;
using ElympicsPlayPad.ExternalCommunicators.Authentication;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;
using ElympicsPlayPad.Tests.Lobby.Common.MockSetup;
using ElympicsPlayPad.Wrappers;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ElympicsPlayPad.Tests
{
    public class TestStandaloneAuthenticationCommunicator
    {
        private static readonly Guid UserId = new("00000000-0000-0000-0000-000000000042");

        private GameObject _gameObject;
        private StandaloneAuthenticationCommunicator _sut;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject(nameof(TestStandaloneAuthenticationCommunicator));
            _sut = _gameObject.AddComponent<StandaloneAuthenticationCommunicator>();
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_gameObject);

        [UnityTest]
        public IEnumerator HandshakeStartsAGuestInLobbyMode() => UniTask.ToCoroutine(async () =>
        {
            var handshake = await _sut.InitializationMessage("gameId", "gameName", "version", "sdk", "playpad");

            Assert.That(handshake.LaunchMode, Is.EqualTo(LaunchMode.Lobby | LaunchMode.Gameplay));
            Assert.That(handshake.Capabilities, Is.EqualTo(Capabilities.Guest));
            Assert.That(handshake.ClosestRegion, Is.Empty);
        });

        [UnityTest]
        public IEnumerator AuthenticateLogsInWithTheLobbyClientSecret() => UniTask.ToCoroutine(async () =>
        {
            var authClient = Substitute.For<IAuthClient>().CreateSuccessIAuthClient(UserId);
            _sut.AuthClient = authClient;

            var authData = await _sut.Authenticate();

            Assert.That(authData.UserId, Is.EqualTo(UserId));
            _ = authClient.Received(1).AuthenticateWithClientSecret(ClientSecretAuth.GetOrCreateClientSecret(), Arg.Any<CancellationToken>());
        });

        [UnityTest]
        public IEnumerator AuthenticateThrowsWhenTheLoginFails() => UniTask.ToCoroutine(async () =>
        {
            _sut.AuthClient = Substitute.For<IAuthClient>().CreateFailureIAuthClient();

            try
            {
                _ = await _sut.Authenticate();
                Assert.Fail("Expected an exception.");
            }
            catch (ElympicsException e)
            {
                StringAssert.Contains("Guest authentication failed", e.Message);
            }
        });

        [UnityTest]
        public IEnumerator ChangeRegionRaisesRegionUpdated() => UniTask.ToCoroutine(async () =>
        {
            string updatedRegion = null;
            _sut.RegionUpdated += region => updatedRegion = region;

            await _sut.ChangeRegion("warsaw");

            Assert.That(updatedRegion, Is.EqualTo("warsaw"));
        });
    }
}
