using System;
using System.IO;
using System.Text;
using ElympicsPlayPad.Editor.Build;
using NUnit.Framework;
using TemplateAction = ElympicsPlayPad.Editor.Build.PlayPadWebGLTemplateInstaller.TemplateAction;

namespace ElympicsPlayPad.Editor.Tests
{
    public class TestPlayPadWebGLTemplateInstaller
    {
        private const string Shipped = "shipped";
        private const string Previous = "previous";
        private const string Unstamped = "legacy";

        private string _directory;

        [SetUp]
        public void CreateDirectory()
        {
            _directory = Path.Combine(Path.GetTempPath(), "PlayPadTemplateHash_" + Guid.NewGuid().ToString("N"));
            _ = Directory.CreateDirectory(_directory);
        }

        [TearDown]
        public void DeleteDirectory() => Directory.Delete(_directory, true);

        [TestCase(null, null, ExpectedResult = nameof(TemplateAction.Install), TestName = "Missing copy is installed")]
        [TestCase(Shipped, null, ExpectedResult = nameof(TemplateAction.UpToDate), TestName = "Unstamped current copy is up to date")]
        [TestCase(Shipped, Previous, ExpectedResult = nameof(TemplateAction.UpToDate), TestName = "Copy matching the package is up to date")]
        [TestCase(Previous, Previous, ExpectedResult = nameof(TemplateAction.Refresh), TestName = "Unmodified stamped copy is refreshed")]
        [TestCase("edited", Previous, ExpectedResult = nameof(TemplateAction.KeepCustomized), TestName = "Edited stamped copy is kept")]
        [TestCase("legacy", null, ExpectedResult = nameof(TemplateAction.Refresh), TestName = "Unstamped previously shipped copy is refreshed")]
        [TestCase("edited", null, ExpectedResult = nameof(TemplateAction.KeepCustomized), TestName = "Unstamped unknown copy is kept")]
        public string Decide(string installedHash, string stampHash) =>
            PlayPadWebGLTemplateInstaller.Decide(installedHash, stampHash, Shipped, Unstamped).ToString();

        [Test]
        public void HashIgnoresLineEndingsOfTextFiles()
        {
            Write("index.html", "<html>\n<body></body>\n</html>\n");
            var lf = PlayPadWebGLTemplateInstaller.ComputeHash(_directory);
            Write("index.html", "<html>\r\n<body></body>\r\n</html>\r\n");
            Assert.That(PlayPadWebGLTemplateInstaller.ComputeHash(_directory), Is.EqualTo(lf));
        }

        [Test]
        public void HashKeepsCarriageReturnsInBinaryFiles()
        {
            WriteBytes("TemplateData/favicon.ico", new byte[] { 1, (byte)'\r', 2 });
            var withCr = PlayPadWebGLTemplateInstaller.ComputeHash(_directory);
            WriteBytes("TemplateData/favicon.ico", new byte[] { 1, 2 });
            Assert.That(PlayPadWebGLTemplateInstaller.ComputeHash(_directory), Is.Not.EqualTo(withCr));
        }

        [Test]
        public void HashIgnoresMetaAndHiddenFiles()
        {
            Write("index.html", "a");
            var before = PlayPadWebGLTemplateInstaller.ComputeHash(_directory);
            Write("index.html.meta", "guid: 1");
            Write(".DS_Store", "x");
            Assert.That(PlayPadWebGLTemplateInstaller.ComputeHash(_directory), Is.EqualTo(before));
        }

        [Test]
        public void HashChangesWithContentAndFileNames()
        {
            Write("assets/index-A.js", "a");
            var original = PlayPadWebGLTemplateInstaller.ComputeHash(_directory);
            Write("assets/index-A.js", "b");
            var edited = PlayPadWebGLTemplateInstaller.ComputeHash(_directory);
            File.Move(Path.Combine(_directory, "assets/index-A.js"), Path.Combine(_directory, "assets/index-B.js"));
            var renamed = PlayPadWebGLTemplateInstaller.ComputeHash(_directory);
            Assert.That(new[] { original, edited, renamed }, Is.Unique);
        }

        [Test]
        public void SyncKeepsMetaOfKeptFilesAndRemovesStaleOnes()
        {
            var source = Path.Combine(_directory, "source");
            var destination = Path.Combine(_directory, "destination");
            Write("source/index.html", "new");
            Write("source/assets/new.js", "n");
            Write("destination/index.html", "old");
            Write("destination/index.html.meta", "guid: kept");
            Write("destination/assets/old.js", "o");
            Write("destination/assets/old.js.meta", "guid: stale");
            Write("destination/gone/old.css", "o");
            Write("destination/gone.meta", "guid: stale");

            PlayPadWebGLTemplateInstaller.SyncDirectory(source, destination);

            Assert.That(File.ReadAllText(Path.Combine(destination, "index.html")), Is.EqualTo("new"));
            Assert.That(File.ReadAllText(Path.Combine(destination, "index.html.meta")), Is.EqualTo("guid: kept"));
            Assert.That(File.Exists(Path.Combine(destination, "assets/new.js")));
            Assert.That(File.Exists(Path.Combine(destination, "assets/old.js")), Is.False);
            Assert.That(File.Exists(Path.Combine(destination, "assets/old.js.meta")), Is.False);
            Assert.That(Directory.Exists(Path.Combine(destination, "gone")), Is.False);
            Assert.That(File.Exists(Path.Combine(destination, "gone.meta")), Is.False);
        }

        private void Write(string relativePath, string content) => WriteBytes(relativePath, Encoding.UTF8.GetBytes(content));

        private void WriteBytes(string relativePath, byte[] content)
        {
            var path = Path.Combine(_directory, relativePath);
            _ = Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, content);
        }
    }
}
