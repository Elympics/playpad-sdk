using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics.ElympicsSystems.Internal;
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
    public class RequestMessageDispatcherTests : ElympicsMonoBaseTest
    {
        private RequestMessageDispatcher _sut;
        private int _ticketCounter;

        public override string SceneName => "ElympicsEmptyTestScene";
        public override bool RequiresElympicsConfig => false;

        [OneTimeSetUp]
        public new void Setup()
        {
            var logger = new ElympicsLoggerContext(Guid.Empty);
            _sut = new RequestMessageDispatcher(logger);
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
            var task = _sut.RequestUniTaskOrThrow<HandshakeResponse>(ticket, default);
            _ = await task;
            Assert.AreEqual(0, _sut.TicketStatus.Count);

        });

        [UnityTest]
        public IEnumerator Test_Request_DoubleResponse_LogError() => UniTask.ToCoroutine(async () =>
        {
            var ticket = _ticketCounter++;
            _sut.RegisterTicket(ticket);
            var response = GenerateHandshakeResponse(ticket, 0);
            _sut.OnResponseObjectReceived(response);
            LogAssert.Expect(LogType.Error, new Regex("Status map already contains response"));
            _sut.OnResponseObjectReceived(response);
        });


        [UnityTest]
        public IEnumerator Test_Response_WithoutTicket() => UniTask.ToCoroutine(async () =>
        {
            LogAssert.Expect(LogType.Error, new Regex("Did not found ticketStatus"));
            var response = GenerateHandshakeResponse(0, 0);
            _sut.OnResponseObjectReceived(response);
            Assert.AreEqual(0, _sut.TicketStatus.Count);

        });

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
            var task = _sut.RequestUniTaskOrThrow<HandshakeResponse>(ticket, default);
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
            var task = _sut.RequestUniTaskOrThrow<HandshakeResponse>(ticket, cts.Token);
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
            cts.Dispose();

        });

        [TearDown]
        public void Cleanup()
        {
            _ticketCounter = 0;
            _ = _sut.SetTimeoutLenght(TimeSpan.FromSeconds(10 * 60));
            _sut.Reset();
        }

        public string GenerateHandshakeResponse(int ticket, int status)
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
