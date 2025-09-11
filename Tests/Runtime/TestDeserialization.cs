using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Responses;
using ElympicsPlayPad.Protocol.VoidMessages;
using ElympicsPlayPad.Protocol.WebMessages;
using ElympicsPlayPad.Utility;
using NUnit.Framework;
using UnityEngine;

namespace ElympicsPlayPad.Tests
{
    [Category("Deserialization")]
    public class TestDeserialization
    {
        private readonly JsCommunicationFactory _factory = new();
        private const string User1Guid = "00000000-0000-0000-0000-000000000001";
        private const string MatchId = "00000000-0000-0000-0000-000000000002";


        private const string ResponseFormat = @"{{
  ""protocolVersion"": ""0.1.0"",
  ""ticket"": {0},
  ""type"": ""Handshake"",
  ""status"": {1},
  ""response"": ""{2}""
}}";

        private const string TestResponse = "testResponse";
        private const int TestTicket = 10;
        private const int TestStatus = 10;

        [Test]
        public void DoHandshake()
        {
            var toDeserialize = string.Format(ResponseFormat, TestTicket, TestStatus, TestResponse);
            var result = JsonUtility.FromJson<ResponseMessage>(toDeserialize);
            Assert.AreEqual(result.response, TestResponse);
            Assert.AreEqual(result.ticket, TestTicket);
            Assert.AreEqual(result.status, TestStatus);
        }

        [Test]
        public void SendSystemInfo()
        {
            var systemInfoDataMessage = new SystemInfoDataMessage
            {
                userId = User1Guid,
                matchId = MatchId,
                systemInfoData = SystemInfoDataFactory.GetSystemInfoData(),
            };
            _ = _factory.GetVoidMessageJson<SystemInfoDataMessage>(VoidMessageTypes.SystemInfoData, systemInfoDataMessage);
        }

        [Test]
        public void WebMessageTest_NullVirtualDeposit()
        {
            const string json = "{\"deposits\":null}";
            var deserialized = JsonUtility.FromJson<VirtualDepositUpdatedMessage>(json);
            Assert.NotNull(deserialized.deposits);
            Assert.AreEqual(0, deserialized.deposits.Length);
        }

    }
}
