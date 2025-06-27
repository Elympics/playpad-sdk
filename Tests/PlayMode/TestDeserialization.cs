using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Responses;
using ElympicsPlayPad.Protocol.VoidMessages;
using ElympicsPlayPad.Protocol.WebMessages;
using ElympicsPlayPad.Utility;
using NUnit.Framework;
using UnityEngine;

namespace ElympicsPlayPad.Tests.PlayMode
{
    [Category("Deserialization")]
    public class TestDeserialization
    {
        private JsCommunicationFactory _factory = new();
        private string _user1Guid = "00000000-0000-0000-0000-000000000001";
        private string _matchId = "00000000-0000-0000-0000-000000000002";


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

        public readonly string handshake = @"{
    ""capabilities"": 5,
    ""device"": ""desktop"",
    ""environment"": ""Stg"",
    ""closestRegion"": ""warsaw"",
    ""authData"": {
      ""jwt"": ""eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI5NzEzYjMyMi0zMzQ1LTQwMmYtOWI3MS0xNzcxMzJmNzM0NjIiLCJhdXRoLXR5cGUiOiJjbGllbnQtc2VjcmV0IiwibmJmIjoxNzI5NTAxMjk5LCJleHAiOjE3Mjk1NDQ0OTksImlhdCI6MTcyOTUwMTI5OX0.ldjn9LN2z5p3iDkyCsa3owbS7aONdnRuuGfG0G-Ya_tZjS3mBfQmRHHSmjzS_kDb8M_tv2tDyF1X2xZHuHYk-X2BKxeWYF7wiTL4pchcrdHU55Yk1MOwq3oH8xBlU8IS--65h22DtWkm0K-jk-j9vfRYpmeuS0i_tai0rx7iME0lbQezeO-cIQgOvPHH66-gfjqgMfKcaSgumHVjP-IhMt_MXsGINH9OFCt00xdSJEFsvL1SxWTtRVtj_z_UrszRA91cYtlUK31JW20pV32Js1fG-ImDAP0PMP7WyrLrWyfA4kActMlLNerppJJiyKPQ6R3bBxj7JtYhbmGnj-sTut8qYYMHUa27g61lBL-UKY9f7enGp8sylmECZOG-05aK4SqNacPLPAtH6K__NLywonOIDJiuDXT2EXLzu8BWta1BzG8lsDuiWjEQfge2g9tDXkRJu4QMRfRlvOozGtr7PRcgaM90v8_FXu09UkqyiCIz03Y14QLsmW5nlbn4Xa8yWmPbf8OouaSYgNGmpRPGdi-CMV_WCEX35M7YOQ4_zMk0N3SKbLAAONdFPgY1GulA3bx1viiZRKDoI67PL5q4uFQZPrZEuoiZ8myg_bjT72wLg3E3gdpehVj6BxlbnEFfZp3glErqmVHJi2cbyf2fDHb0O2LfV_17yDkZcfdo9L4"",
      ""userId"": ""9713b322-3345-402f-9b71-177132f73462"",
      ""nickname"": ""Guest-46249""
    }
  }";


        [Test]
        public void Handshake()
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
                userId = _user1Guid,
                matchId = _matchId,
                systemInfoData = SystemInfoDataFactory.GetSystemInfoData()
            };
            var message = _factory.GetVoidMessageJson<SystemInfoDataMessage>(VoidMessageTypes.SystemInfoData, systemInfoDataMessage);
        }

        [Test]
        public void WebMessageTest_NullVirtualDeposit()
        {
            var json = "{\"deposits\":null}";
            var deserialized = JsonUtility.FromJson<VirtualDepositUpdatedMessage>(json);
            Assert.NotNull(deserialized.deposits);
            Assert.AreEqual(0,deserialized.deposits.Length);
        }

    }
}
