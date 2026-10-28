#nullable enable

using ElympicsPlayPad.Editor.Build;
using NUnit.Framework;
using UnityEditor.Build;

namespace ElympicsPlayPad.Editor.Tests
{
    public class TestPlayPadPreProcessBuild
    {
        [TestCase("eu", "us", ExpectedResult = "eu", TestName = "Environment variable wins")]
        [TestCase("", " us ", ExpectedResult = "us", TestName = "Custom value is used without the variable")]
        [TestCase("  ", "us", ExpectedResult = "us", TestName = "Blank variable is ignored")]
        public string RequireRegion(string fromEnvironment, string fromCustomValue) =>
            PlayPadPreProcessBuild.RequireRegion(fromEnvironment, fromCustomValue);

        [TestCase(null, null)]
        [TestCase("", " ")]
        public void RequireRegionFailsTheBuildWithoutARegion(string? fromEnvironment, string? fromCustomValue)
        {
            var exception = Assert.Throws<BuildFailedException>(() => PlayPadPreProcessBuild.RequireRegion(fromEnvironment, fromCustomValue));
            Assert.That(exception.Message, Does.Contain(PlayPadPreProcessBuild.SetTemplateValuesCommand + " --region").And.Contain(PlayPadPreProcessBuild.Region.Variable));
        }

        [TestCase("https://example.com/a?b=c&d=e", ExpectedResult = true)]
        [TestCase("", ExpectedResult = true)]
        [TestCase("a\"b", ExpectedResult = false)]
        [TestCase("a\\b", ExpectedResult = false)]
        [TestCase("</script>", ExpectedResult = false)]
        [TestCase("a\nb", ExpectedResult = false)]
        public bool IsSafeTemplateValue(string value) => PlayPadPreProcessBuild.IsSafeTemplateValue(value);
    }
}
