using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics.Tests;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Responses;
using ElympicsPlayPad.Tests.PlayMode.Mocks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ElympicsPlayPad.Tests.PlayMode
{
    public class RequestMessageDispatcherTests : ElympicsMonoBaseTest
    {
        private RequestMessageDispatcher _sut;
        private JsCommunicatorRetrieverMock _jsMock;
        private int _ticketCounter;

        public override string SceneName => "ElympicsEmptyTestScene";
        public override bool RequiresElympicsConfig => false;

        [OneTimeSetUp]
        public void Setup()
        {
            _jsMock = new JsCommunicatorRetrieverMock();
            _sut = new RequestMessageDispatcher(_jsMock, default);
        }

        [UnityTest]
        public IEnumerator Test_Request_HappyPath() => UniTask.ToCoroutine(async () =>
        {
            var ticket = _ticketCounter++;
            _sut.RegisterTicket(ticket);
            var task = _sut.RequestUniTaskOrThrow<HandshakeResponse>(ticket, default);
            _jsMock.SendHandshakeResponse(ticket, 0);
            _ = await task;
            Assert.AreEqual(0, _sut.TicketStatus.Count);

        });

        [UnityTest]
        public IEnumerator Test_Request_HappyPath_RequestAlreadyWaiting() => UniTask.ToCoroutine(async () =>
        {
            var ticket = _ticketCounter++;
            _sut.RegisterTicket(ticket);
            _jsMock.SendHandshakeResponse(ticket, 0);
            var task = _sut.RequestUniTaskOrThrow<HandshakeResponse>(ticket, default);
            _ = await task;
            Assert.AreEqual(0, _sut.TicketStatus.Count);

        });

        [UnityTest]
        public IEnumerator Test_Request_DoubleResponse_LogError() => UniTask.ToCoroutine(async () =>
        {
            var ticket = _ticketCounter++;
            _sut.RegisterTicket(ticket);
            _jsMock.SendHandshakeResponse(ticket, 0);
            var exceptionThrown = false;
            LogAssert.Expect(LogType.Error, new Regex("Status map already contains response"));
            _jsMock.SendHandshakeResponse(ticket, 0);
        });


        [UnityTest]
        public IEnumerator Test_Response_WithoutTicket() => UniTask.ToCoroutine(async () =>
        {
            LogAssert.Expect(LogType.Error, new Regex("Did not found ticketStatus"));
            _jsMock.SendHandshakeResponse(0, 0);
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
            _jsMock.SendHandshakeResponse(ticket, 0);
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
            _jsMock.SendHandshakeResponse(ticket, 1);
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
            _jsMock.SendHandshakeResponse(ticket, 1);
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
            _sut.SetTimeoutLenght(TimeSpan.FromMilliseconds(10));
            var ticket = _ticketCounter++;
            _sut.RegisterTicket(ticket);
            var task = _sut.RequestUniTaskOrThrow<HandshakeResponse>(ticket, default);
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
            _jsMock.SendHandshakeResponse(ticket, 0);
            Assert.AreEqual(0, _sut.TicketStatus.Count);
        });

        [UnityTest]
        public IEnumerator Test_Request_LinkedCancellation_Timeout() => UniTask.ToCoroutine(async () =>
        {
            var cts = new CancellationTokenSource();
            _sut.SetTimeoutLenght(TimeSpan.FromMilliseconds(10));
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
            _jsMock.SendHandshakeResponse(ticket, 0);
            Assert.AreEqual(0, _sut.TicketStatus.Count);
            cts.Dispose();

        });

        [TearDown]
        public void Cleanup()
        {
            _ticketCounter = 0;
            _sut.SetTimeoutLenght(TimeSpan.FromSeconds(10 * 60));
            _sut.Reset();
        }
    }
}
