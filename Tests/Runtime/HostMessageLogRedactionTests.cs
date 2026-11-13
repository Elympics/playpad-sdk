using ElympicsPlayPad.ExternalCommunicators.WebCommunication;
using NUnit.Framework;

namespace ElympicsPlayPad.Tests
{
    [TestFixture]
    internal class HostMessageLogRedactionTests
    {
        [Test]
        public void SecretValuesInPlainJsonShouldBeMasked()
        {
            const string json = "{\"jwt\": \"eyJ.abc.def\",\"userId\":\"u1\",\"userSecret\":\"s-123\",\"clientSecret\":\"c-1\",\"jwtToken\":\"eyJ.x.y\"}";

            var redacted = HostMessageLogRedaction.Redact(json);

            Assert.AreEqual("{\"jwt\": \"***\",\"userId\":\"u1\",\"userSecret\":\"***\",\"clientSecret\":\"***\",\"jwtToken\":\"***\"}", redacted);
        }

        [Test]
        public void SecretValuesInJsonNestedInStringShouldBeMasked()
        {
            const string json = "{\"ticket\":1,\"type\":\"GetAuthentication\",\"status\":0,"
                + "\"response\":\"{\\\"jwt\\\":\\\"eyJ.abc.def\\\",\\\"nickname\\\":\\\"jwt\\\"}\"}";

            var redacted = HostMessageLogRedaction.Redact(json);

            Assert.AreEqual("{\"ticket\":1,\"type\":\"GetAuthentication\",\"status\":0,"
                + "\"response\":\"{\\\"jwt\\\":\\\"***\\\",\\\"nickname\\\":\\\"jwt\\\"}\"}", redacted);
        }

        [TestCase(null, "")]
        [TestCase("", "")]
        [TestCase("{\"type\":\"jwt\",\"payload\":{\"jwtX\":\"keep\"}}", "{\"type\":\"jwt\",\"payload\":{\"jwtX\":\"keep\"}}")]
        [TestCase("{\"jwt\":null}", "{\"jwt\":null}")]
        public void TextWithoutSecretStringValuesShouldBeUnchanged(string input, string expected) =>
            Assert.AreEqual(expected, HostMessageLogRedaction.Redact(input));
    }
}
