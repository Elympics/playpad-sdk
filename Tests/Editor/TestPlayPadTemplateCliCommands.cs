using System;
using System.IO;
using System.Text;
using ElympicsPlayPad.Editor.Build;
using ElympicsPlayPad.Editor.UnityCli;
using NUnit.Framework;

namespace ElympicsPlayPad.Editor.Tests
{
    /// <summary>Runs on temporary template folders; doesn't set template custom values, which would change the host project's ProjectSettings.</summary>
    [Category("UnityCli")]
    public class TestPlayPadTemplateCliCommands
    {
        private string _directory;

        private static readonly string InstalledIndex = $"templates/{PlayPadWebGLTemplateInstaller.TemplateName}/index.html";

        private string Installed => Path.Combine(_directory, InstalledIndex);

        [SetUp]
        public void CreateTemplates()
        {
            _directory = Path.Combine(Path.GetTempPath(), "PlayPadTemplateCli_" + Guid.NewGuid().ToString("N"));
            Write("source/index.html", "shipped");
            PlayPadWebGLTemplateInstaller.SourcePathOverride = Path.Combine(_directory, "source");
            PlayPadWebGLTemplateInstaller.TemplatesRootOverride = Path.Combine(_directory, "templates");
        }

        [TearDown]
        public void DeleteTemplates()
        {
            PlayPadWebGLTemplateInstaller.SourcePathOverride = null;
            PlayPadWebGLTemplateInstaller.TemplatesRootOverride = null;
            Directory.Delete(_directory, true);
        }

        [Test]
        public void StatusReportsAMissingTemplate()
        {
            var result = PlayPadTemplateCliCommands.Status();
            Assert.That(Get(result, "action"), Is.EqualTo("install"));
            Assert.That(Get(result, "installed"), Is.False);
        }

        [Test]
        public void InstallInstallsAMissingTemplate()
        {
            var result = PlayPadTemplateCliCommands.Install();
            Assert.That(Get(result, "status"), Is.EqualTo("installed"));
            Assert.That(File.ReadAllText(Installed), Is.EqualTo("shipped"));
            Assert.That(Get(PlayPadTemplateCliCommands.Status(), "action"), Is.EqualTo("up_to_date"));
        }

        [Test]
        public void InstallLeavesAnUpToDateTemplate()
        {
            _ = PlayPadTemplateCliCommands.Install();
            Assert.That(Get(PlayPadTemplateCliCommands.Install(), "status"), Is.EqualTo("up_to_date"));
        }

        [Test]
        public void InstallRefusesACustomizedTemplate()
        {
            _ = PlayPadTemplateCliCommands.Install();
            Write(InstalledIndex, "edited");

            AssertCodeAndFix(Assert.Catch(() => PlayPadTemplateCliCommands.Install()), PlayPadTemplateCliCommands.TemplateCustomized);
            Assert.That(File.ReadAllText(Installed), Is.EqualTo("edited"));
        }

        [Test]
        public void ForcedInstallReplacesACustomizedTemplate()
        {
            _ = PlayPadTemplateCliCommands.Install();
            Write(InstalledIndex, "edited");

            Assert.That(Get(PlayPadTemplateCliCommands.Install(force: true), "previousAction"), Is.EqualTo("keep_customized"));
            Assert.That(File.ReadAllText(Installed), Is.EqualTo("shipped"));
        }

        [TestCase(" ", "", "")]
        [TestCase("", "\t", "")]
        [TestCase("", "", "  ")]
        public void ValuesRejectBlankArguments(string environment, string region, string queue) =>
            AssertCodeAndFix(Assert.Catch(() => PlayPadTemplateCliCommands.Values(environment, region, queue)), "ELYMPICS_INVALID_ARGUMENT");

        [Test]
        public void ValuesWithoutArgumentsChangeNothing() =>
            Assert.That(Get(PlayPadTemplateCliCommands.Values(), "status"), Is.EqualTo("ok"));

        private static object Get(object result, string property) => result.GetType().GetProperty(property)!.GetValue(result);

        private static void AssertCodeAndFix(Exception exception, string code)
        {
            Assert.That(exception, Is.Not.Null, $"Expected {code}.");
            Assert.That(exception.Message, Does.StartWith(code + ": "));
            Assert.That(exception.Message, Does.Match(@" Fix: \S"));
        }

        private void Write(string relativePath, string content)
        {
            var path = Path.Combine(_directory, relativePath);
            _ = Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, Encoding.UTF8.GetBytes(content));
        }
    }
}
