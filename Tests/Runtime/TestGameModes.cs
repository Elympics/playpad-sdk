#nullable enable
using System;
using ElympicsPlayPad.ExternalCommunicators.Authentication;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;
using ElympicsPlayPad.Protocol.Responses;
using NUnit.Framework;
using UnityEngine;

namespace ElympicsPlayPad.Tests
{
    public class TestGameModes
    {
        private string? _originalGameModeVariable;

        [SetUp]
        public void RememberGameModeVariable() => _originalGameModeVariable = Environment.GetEnvironmentVariable(GameModeOverride.EnvironmentVariableName);

        [TearDown]
        public void RestoreGameModeVariable() => Environment.SetEnvironmentVariable(GameModeOverride.EnvironmentVariableName, _originalGameModeVariable);

        [TestCase(null, null)]
        [TestCase("", null)]
        [TestCase("  ", null)]
        [TestCase("standard", null)]
        [TestCase(" standard ", null)]
        [TestCase("Standard", "Standard")]
        [TestCase("onboarding", "onboarding")]
        [TestCase(" onboarding ", "onboarding")]
        public void Normalize_ReturnsTrimmedModeOrNullForStandard(string? raw, string? expected) =>
            Assert.AreEqual(expected, GameModes.Normalize(raw));

        [Test]
        public void CreateHandshakeInfoWithoutGameModeGivesNull()
        {
            var info = new HandshakeInfo(false, Capabilities.None, "Dev", "warsaw", FeatureAccess.None, new UserPrefsInfo(Array.Empty<string>()), LaunchMode.Gameplay);

            Assert.IsNull(info.GameMode);
        }

        [Test]
        public void CreateHandshakeInfoWithGameModeKeepsIt()
        {
            var info = new HandshakeInfo(false, Capabilities.None, "Dev", "warsaw", FeatureAccess.None, new UserPrefsInfo(Array.Empty<string>()), LaunchMode.Gameplay, "onboarding");

            Assert.AreEqual("onboarding", info.GameMode);
        }

        [Test]
        public void DeserializeHandshakeResponseWithGameModeKeepsIt()
        {
            var response = JsonUtility.FromJson<HandshakeResponse>("{\"environment\":\"Stg\",\"launchMode\":1,\"gameMode\":\"onboarding_ghost\"}");

            Assert.AreEqual("onboarding_ghost", GameModes.Normalize(response.gameMode));
        }

        [Test]
        public void DeserializeHandshakeResponseWithoutGameModeGivesStandard()
        {
            var response = JsonUtility.FromJson<HandshakeResponse>("{\"environment\":\"Stg\",\"launchMode\":1}");

            Assert.IsNull(GameModes.Normalize(response.gameMode));
        }

        // The JoyFlick handshake response recorded in the host hand-off, left open: each test closes it, optionally after a gameMode member.
        private const string RecordedHandshakeJson = "{\"capabilities\":0,\"featureAccess\":0,\"launchMode\":1,\"device\":\"desktop\",\"environment\":\"Stg\",\"closestRegion\":\"stg\","
            + "\"heartbeatIntervalMs\":2000,\"userPrefs\":{\"languages\":[\"en\"]},\"gameVersionId\":\"\",\"fleetName\":\"\"";

        [Test]
        public void CreateHandshakeInfoFromRecordedOnboardingResponseGivesOnboarding()
        {
            var response = JsonUtility.FromJson<HandshakeResponse>(RecordedHandshakeJson + ",\"gameMode\":\"onboarding\"}");

            var info = WebGLExternalAuthenticator.CreateHandshakeInfo(response);

            Assert.AreEqual("onboarding", info.GameMode);
            Assert.AreEqual("Stg", info.Environment);
            Assert.AreEqual(LaunchMode.Gameplay, info.LaunchMode);
        }

        [Test]
        public void CreateHandshakeInfoFromRecordedResponseWithoutGameModeGivesNull()
        {
            var response = JsonUtility.FromJson<HandshakeResponse>(RecordedHandshakeJson + "}");

            Assert.IsNull(WebGLExternalAuthenticator.CreateHandshakeInfo(response).GameMode);
        }

        [Test]
        public void CreateHandshakeInfoFromRecordedStandardResponseGivesNull()
        {
            var response = JsonUtility.FromJson<HandshakeResponse>(RecordedHandshakeJson + ",\"gameMode\":\"standard\"}");

            Assert.IsNull(WebGLExternalAuthenticator.CreateHandshakeInfo(response).GameMode);
        }

#if UNITY_EDITOR || !UNITY_WEBGL
        [Test]
        public void ReadOverride_FromEnvironmentVariable_ReturnsValue()
        {
            Environment.SetEnvironmentVariable(GameModeOverride.EnvironmentVariableName, "onboarding");

            Assert.AreEqual("onboarding", GameModeOverride.Read());
        }

        [Test]
        public void ReadOverride_WithoutEnvironmentVariable_ReturnsNull()
        {
            Environment.SetEnvironmentVariable(GameModeOverride.EnvironmentVariableName, null);

            Assert.IsNull(GameModeOverride.Read());
        }
#endif
    }
}
