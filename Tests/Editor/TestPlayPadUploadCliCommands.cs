#nullable enable

using System;
using System.IO;
using Elympics;
using Elympics.Editor;
using Elympics.Editor.UnityCli;
using ElympicsPlayPad.Editor.UnityCli;
using NUnit.Framework;

namespace ElympicsPlayPad.Editor.Tests
{
    [Category("UnityCli")]
    public class TestPlayPadUploadCliCommands
    {
        [Test]
        public void UploadClientBuildNeedsConfirm() =>
            PlayPadCliAssert.HasCodeAndFix(Assert.CatchAsync(async () => await PlayPadUploadCliCommands.UploadClientBuild("Builds/WebGL", "1.0")), ElympicsCliErrors.ConfirmRequired);

        [Test]
        public void UploadClientBuildValidatesTheVersion() =>
            PlayPadCliAssert.HasCodeAndFix(Assert.CatchAsync(async () => await PlayPadUploadCliCommands.UploadClientBuild("Builds/WebGL", "1.0 beta", confirm: true)), ElympicsCliErrors.InvalidArgument);

        [Test]
        public void UploadClientBuildNeedsAConfig()
        {
            var original = ElympicsConfig.PathInResources;
            ElympicsConfig.PathInResources = "Elympics/NoSuchConfig_" + Guid.NewGuid().ToString("N");
            try
            {
                PlayPadCliAssert.HasCodeAndFix(Assert.CatchAsync(async () => await PlayPadUploadCliCommands.UploadClientBuild("NoSuchBuild", "1.0", gameId: Guid.NewGuid().ToString(), confirm: true)),
                    ElympicsCliErrors.NoConfig);
            }
            finally
            {
                ElympicsConfig.PathInResources = original;
            }
        }

        [TestCase("../NoSuchBuild")]
        [TestCase("Builds/../../NoSuchBuild")]
        public void UploadClientBuildRejectsADirectoryOutsideTheProject(string path) =>
            PlayPadCliAssert.HasCodeAndFix(Assert.CatchAsync(async () => await PlayPadUploadCliCommands.UploadClientBuild(path, "1.0", gameId: Guid.NewGuid().ToString(), confirm: true)), ElympicsCliErrors.InvalidArgument);

        [Test]
        public void UploadStreamingAssetsNeedsConfirm() =>
            PlayPadCliAssert.HasCodeAndFix(Assert.CatchAsync(async () => await PlayPadUploadCliCommands.UploadStreamingAssets("Assets", "1.0")), ElympicsCliErrors.ConfirmRequired);

        [Test]
        public void UploadStreamingAssetsRejectsADirectoryOutsideTheProject() =>
            PlayPadCliAssert.HasCodeAndFix(Assert.CatchAsync(async () => await PlayPadUploadCliCommands.UploadStreamingAssets(Path.GetTempPath(), "1.0", confirm: true)), ElympicsCliErrors.InvalidArgument);

        [Test]
        public void UploadStreamingAssetsAllowsAnExternalDirectoryWhenAsked() =>
            PlayPadCliAssert.HasCodeAndFix(Assert.CatchAsync(async () => await PlayPadUploadCliCommands.UploadStreamingAssets(Path.Combine(Path.GetTempPath(), "NoSuchContent_" + Guid.NewGuid().ToString("N")),
                "1.0", Guid.NewGuid().ToString(), confirm: true, allowExternalPath: true)), ElympicsCliErrors.BuildDirNotFound);

        [Test]
        public void UploadStreamingAssetsNeedsAnExistingDirectory() =>
            PlayPadCliAssert.HasCodeAndFix(Assert.CatchAsync(async () => await PlayPadUploadCliCommands.UploadStreamingAssets("NoSuchContent_" + Guid.NewGuid().ToString("N"), "1.0", Guid.NewGuid().ToString(), confirm: true)),
                ElympicsCliErrors.BuildDirNotFound);

        [Test]
        public void StreamingAssetsLayoutDefaultsToAddressableVariants()
        {
            Assert.That(PlayPadUploadCliCommands.ParseLayout(""), Is.EqualTo(StreamingAssetsLayout.AddressableVariants));
            Assert.That(PlayPadUploadCliCommands.ParseLayout("unstructuredassets"), Is.EqualTo(StreamingAssetsLayout.UnstructuredAssets));
            PlayPadCliAssert.HasCodeAndFix(Assert.Catch(() => PlayPadUploadCliCommands.ParseLayout("other")), ElympicsCliErrors.InvalidArgument);
        }
    }
}
