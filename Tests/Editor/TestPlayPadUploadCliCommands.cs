#nullable enable

using System;
using Elympics;
using ElympicsPlayPad.Editor.UnityCli;
using NUnit.Framework;

namespace ElympicsPlayPad.Editor.Tests
{
    [Category("UnityCli")]
    public class TestPlayPadUploadCliCommands
    {
        [Test]
        public void UploadClientBuildNeedsConfirm() =>
            AssertCodeAndFix(Assert.CatchAsync(async () => await PlayPadUploadCliCommands.UploadClientBuild("Builds/WebGL", "1.0")), "ELYMPICS_CONFIRM_REQUIRED");

        [Test]
        public void UploadClientBuildValidatesTheVersion() =>
            AssertCodeAndFix(Assert.CatchAsync(async () => await PlayPadUploadCliCommands.UploadClientBuild("Builds/WebGL", "1.0 beta", confirm: true)), "ELYMPICS_INVALID_ARGUMENT");

        [Test]
        public void UploadClientBuildNeedsAConfiguredGame() =>
            AssertCodeAndFix(Assert.CatchAsync(async () => await PlayPadUploadCliCommands.UploadClientBuild("NoSuchBuild_" + Guid.NewGuid().ToString("N"), "1.0", gameId: Guid.NewGuid().ToString(), confirm: true)),
                ElympicsConfig.Load() == null ? "ELYMPICS_NO_CONFIG" : "ELYMPICS_GAME_NOT_FOUND");

        [TestCase("../NoSuchBuild")]
        [TestCase("Builds/../../NoSuchBuild")]
        public void UploadClientBuildRejectsADirectoryOutsideTheProject(string path) =>
            AssertCodeAndFix(Assert.CatchAsync(async () => await PlayPadUploadCliCommands.UploadClientBuild(path, "1.0", gameId: Guid.NewGuid().ToString(), confirm: true)), "ELYMPICS_INVALID_ARGUMENT");

        private static void AssertCodeAndFix(Exception? exception, string code)
        {
            Assert.That(exception, Is.Not.Null, $"Expected {code}.");
            Assert.That(exception!.Message, Does.StartWith(code + ": "));
            Assert.That(exception.Message, Does.Match(@" Fix: \S"));
        }
    }
}
