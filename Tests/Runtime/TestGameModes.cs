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
        public void HandshakeInfo_WithoutGameMode_HasNullGameMode()
        {
            var info = new HandshakeInfo(false, Capabilities.None, "Dev", "warsaw", FeatureAccess.None, new UserPrefsInfo(Array.Empty<string>()), LaunchMode.Gameplay);

            Assert.IsNull(info.GameMode);
        }

        [Test]
        public void HandshakeInfo_WithGameMode_KeepsIt()
        {
            var info = new HandshakeInfo(false, Capabilities.None, "Dev", "warsaw", FeatureAccess.None, new UserPrefsInfo(Array.Empty<string>()), LaunchMode.Gameplay, "onboarding");

            Assert.AreEqual("onboarding", info.GameMode);
        }

        [Test]
        public void HandshakeResponse_WithGameMode_DeserializesIt()
        {
            var response = JsonUtility.FromJson<HandshakeResponse>("{\"environment\":\"Stg\",\"launchMode\":1,\"gameMode\":\"onboarding_ghost\"}");

            Assert.AreEqual("onboarding_ghost", GameModes.Normalize(response.gameMode));
        }

        [Test]
        public void HandshakeResponse_WithoutGameMode_IsStandard()
        {
            var response = JsonUtility.FromJson<HandshakeResponse>("{\"environment\":\"Stg\",\"launchMode\":1}");

            Assert.IsNull(GameModes.Normalize(response.gameMode));
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
