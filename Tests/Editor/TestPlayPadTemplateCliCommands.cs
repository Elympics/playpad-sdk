#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Elympics.Editor.UnityCli;
using ElympicsPlayPad.Editor.Build;
using ElympicsPlayPad.Editor.UnityCli;
using NUnit.Framework;

namespace ElympicsPlayPad.Editor.Tests
{
    /// <summary>Runs on temporary template folders; doesn't set template custom values, which would change the host project's ProjectSettings.</summary>
    [Category("UnityCli")]
    public class TestPlayPadTemplateCliCommands
    {
        private string _directory = null!;

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
        public void GetTemplateStatusReportsAMissingTemplate()
        {
            var result = PlayPadTemplateCliCommands.GetTemplateStatus();
            Assert.That(Get(result, "action"), Is.EqualTo("install"));
            Assert.That(Get(result, "installed"), Is.False);
        }

        [Test]
        public void InstallTemplateInstallsAMissingTemplate()
        {
            var result = PlayPadTemplateCliCommands.InstallTemplate();
            Assert.That(Get(result, "status"), Is.EqualTo("installed"));
            Assert.That(File.ReadAllText(Installed), Is.EqualTo("shipped"));
            Assert.That(Get(PlayPadTemplateCliCommands.GetTemplateStatus(), "action"), Is.EqualTo("up_to_date"));
        }

        [Test]
        public void InstallTemplateLeavesAnUpToDateTemplate()
        {
            _ = PlayPadTemplateCliCommands.InstallTemplate();
            Assert.That(Get(PlayPadTemplateCliCommands.InstallTemplate(), "status"), Is.EqualTo("up_to_date"));
        }

        [Test]
        public void InstallTemplateRefusesACustomizedTemplate()
        {
            _ = PlayPadTemplateCliCommands.InstallTemplate();
            Write(InstalledIndex, "edited");

            PlayPadCliAssert.HasCodeAndFix(Assert.Catch(() => PlayPadTemplateCliCommands.InstallTemplate()), PlayPadTemplateCliCommands.TemplateCustomized);
            Assert.That(File.ReadAllText(Installed), Is.EqualTo("edited"));
        }

        [Test]
        public void ForcedInstallReplacesACustomizedTemplate()
        {
            _ = PlayPadTemplateCliCommands.InstallTemplate();
            Write(InstalledIndex, "edited");

            Assert.That(Get(PlayPadTemplateCliCommands.InstallTemplate(force: true), "previousAction"), Is.EqualTo("keep_customized"));
            Assert.That(File.ReadAllText(Installed), Is.EqualTo("shipped"));
        }

        [TestCase(" ", "", "")]
        [TestCase("", "\t", "")]
        [TestCase("", "", "  ")]
        public void SetTemplateValuesRejectsBlankArguments(string environment, string region, string queue) =>
            PlayPadCliAssert.HasCodeAndFix(Assert.Catch(() => PlayPadTemplateCliCommands.SetTemplateValues(environment, region, queue)), ElympicsCliErrors.InvalidArgument);

        [Test]
        public void SetTemplateValuesNeedsAValue() =>
            PlayPadCliAssert.HasCodeAndFix(Assert.Catch(() => PlayPadTemplateCliCommands.SetTemplateValues()), ElympicsCliErrors.InvalidArgument);

        [Test]
        public void GetTemplateValuesReportsEveryValue()
        {
            var result = PlayPadTemplateCliCommands.GetTemplateValues();

            Assert.That(Get(result, "status"), Is.EqualTo("ok"));
            Assert.That(((Dictionary<string, string?>)Get(result, "values")!).Keys, Is.EquivalentTo(PlayPadPreProcessBuild.TemplateValues.Select(value => value.Key)));
        }

        private static object? Get(object result, string property) => result.GetType().GetProperty(property)!.GetValue(result);

        private void Write(string relativePath, string content)
        {
            var path = Path.Combine(_directory, relativePath);
            _ = Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, Encoding.UTF8.GetBytes(content));
        }
    }
}
