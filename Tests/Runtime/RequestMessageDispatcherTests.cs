using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Responses;
using ElympicsPlayPad.Tests.Mocks;
using ElympicsPlayPad.Tests.Runtime.Mocks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ElympicsPlayPad.Tests
{
    public class RequestMessageDispatcherTests : ElympicsMonoProviderBaseTest
    {
        private RequestMessageDispatcher _sut;
        private int _ticketCounter;

        protected override string SceneName => "ElympicsEmptyTestScene";
        protected override bool RequiresTestConfig => false;

        [OneTimeSetUp]
        protected override void OneTimeSetUp()
        {
            base.OneTimeSetUp();
            _sut = new RequestMessageDispatcher();
        }

        [UnityTest]
        public IEnumerator Test_Request_HappyPath() => UniTask.ToCoroutine(async () =>
        {
            var ticket = _ticketCounter++;
            _sut.RegisterTicket(ticket);
            var task = _sut.RequestUniTaskOrThrow<HandshakeResponse>(ticket, default);
            var response = GenerateHandshakeResponse(ticket, 0);
            _sut.OnResponseObjectReceived(response);
            _ = await task;
            Assert.AreEqual(0, _sut.TicketStatus.Count);

        });

        [UnityTest]
        public IEnumerator Test_Request_HappyPath_RequestAlreadyWaiting() => UniTask.ToCoroutine(async () =>
        {
            var ticket = _ticketCounter++;
            _sut.RegisterTicket(ticket);
            var response = GenerateHandshakeResponse(ticket, 0);
            _sut.OnResponseObjectReceived(response);
            var task = _sut.RequestUniTaskOrThrow<HandshakeResponse>(ticket, CancellationToken.None);
            _ = await task;
            Assert.AreEqual(0, _sut.TicketStatus.Count);

        });

        [Test]
        public void Test_Request_DoubleResponse_LogError()
        {
            var ticket = _ticketCounter++;
            _sut.RegisterTicket(ticket);
            var response = GenerateHandshakeResponse(ticket, 0);
            _sut.OnResponseObjectReceived(response);
            LogAssert.Expect(LogType.Error, new Regex("Status map already contains response"));
            _sut.OnResponseObjectReceived(response);
        }


        [Test]
        public void Test_Response_WithoutTicket()
        {
            LogAssert.Expect(LogType.Error, new Regex("Did not found ticketStatus"));
            var response = GenerateHandshakeResponse(0, 0);
            _sut.OnResponseObjectReceived(response);
            Assert.AreEqual(0, _sut.TicketStatus.Count);

        }

        [UnityTest]
        public IEnumerator Test_Request_UserCancelled() => UniTask.ToCoroutine(async () =>
        {
            var cts = new CancellationTokenSource();
            var ticket = _ticketCounter++;
            _sut.RegisterTicket(ticket);
            var task = _sut.RequestUniTaskOrThrow<HandshakeResponse>(ticket, cts.Token);
            cts.Cancel();
            var response = GenerateHandshakeResponse(ticket, 0);
            _sut.OnResponseObjectReceived(response);
            var exceptionThrown = false;
            try
            {
                _ = await task;
            }
            catch (OperationCanceledException)
            {
                exceptionThrown = true;
            }
            Assert.True(exceptionThrown);
            Assert.AreEqual(0, _sut.TicketStatus.Count);

        });

        [UnityTest]
        public IEnumerator Test_Request_WithError() => UniTask.ToCoroutine(async () =>
        {
            var ticket = _ticketCounter++;
            _sut.RegisterTicket(ticket);
            var task = _sut.RequestUniTaskOrThrow<HandshakeResponse>(ticket, CancellationToken.None);
            var response = GenerateHandshakeResponse(ticket, 1);
            _sut.OnResponseObjectReceived(response);
            var exceptionThrown = false;
            try
            {
                _ = await task;
            }
            catch (ResponseException)
            {
                exceptionThrown = true;
            }
            Assert.True(exceptionThrown);
            Assert.AreEqual(0, _sut.TicketStatus.Count);

        });

        [UnityTest]
        public IEnumerator Test_Request_WithError_UserCancelled() => UniTask.ToCoroutine(async () =>
        {
            var cts = new CancellationTokenSource();
            var ticket = _ticketCounter++;
            _sut.RegisterTicket(ticket);
            var task = _sut.RequestUniTaskOrThrow<HandshakeResponse>(ticket, cts.Token);
            cts.Cancel();
            var response = GenerateHandshakeResponse(ticket, 1);
            _sut.OnResponseObjectReceived(response);
            var exceptionThrown = false;
            try
            {
                _ = await task;
            }
            catch (OperationCanceledException)
            {
                exceptionThrown = true;
            }
            Assert.True(exceptionThrown);
            Assert.AreEqual(0, _sut.TicketStatus.Count);

        });

        [UnityTest]
        public IEnumerator Test_Request_Timeout() => UniTask.ToCoroutine(async () =>
        {
            _ = _sut.SetTimeoutLenght(TimeSpan.FromMilliseconds(10));
            var ticket = _ticketCounter++;
            _sut.RegisterTicket(ticket);
            LogAssert.Expect(LogType.Exception, new Regex("Request reached timeout"));
            var task = _sut.RequestUniTaskOrThrow<HandshakeResponse>(ticket, CancellationToken.None);
            await UniTask.Delay(TimeSpan.FromMilliseconds(20));
            var exceptionThrown = false;
            try
            {
                _ = await task;
            }
            catch (ProtocolException)
            {
                exceptionThrown = true;
            }
            Assert.True(exceptionThrown);
            Assert.AreEqual(1, _sut.TicketStatus.Count);
            var response = GenerateHandshakeResponse(ticket, 0);
            _sut.OnResponseObjectReceived(response);
            Assert.AreEqual(0, _sut.TicketStatus.Count);
        });

        [UnityTest]
        public IEnumerator Test_Request_LinkedCancellation_Timeout() => UniTask.ToCoroutine(async () =>
        {
            var cts = new CancellationTokenSource();
            _ = _sut.SetTimeoutLenght(TimeSpan.FromMilliseconds(10));
            var ticket = _ticketCounter++;
            _sut.RegisterTicket(ticket);
            LogAssert.Expect(LogType.Exception, new Regex("Request reached timeout"));
            var task = _sut.RequestUniTaskOrThrow<HandshakeResponse>(ticket, cts.Token);
            await UniTask.Delay(TimeSpan.FromMilliseconds(20), cancellationToken: cts.Token);
            var exceptionThrown = false;
            try
            {
                _ = await task;
            }
            catch (ProtocolException)
            {
                exceptionThrown = true;
            }
            Assert.True(exceptionThrown);
            Assert.AreEqual(1, _sut.TicketStatus.Count);
            var response = GenerateHandshakeResponse(ticket, 0);
            _sut.OnResponseObjectReceived(response);
            Assert.AreEqual(0, _sut.TicketStatus.Count);
            cts.Cancel();
            cts.Dispose();
        });

        [TearDown]
        protected void Cleanup()
        {
            _ticketCounter = 0;
            _ = _sut.SetTimeoutLenght(TimeSpan.FromSeconds(10 * 60));
            _sut.Reset();
        }

        private static string GenerateHandshakeResponse(int ticket, int status)
        {
            var handshakeResponse = GetHandshakeResponse();
            var response = new ResponseMessage
            {
                ticket = ticket,
                type = RequestResponseMessageTypes.Handshake,
                status = status,
                response = JsonUtility.ToJson(handshakeResponse),
            };

            return JsonUtility.ToJson(response);
        }

        private static HandshakeResponse GetHandshakeResponse() => new()
        {
            error = null,
            device = "mobile",
            environment = "PROD",
            capabilities = 3,
            featureAccess = 5,
            closestRegion = "mumbai",
        };
    }
}
