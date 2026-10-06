#nullable enable

using System;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol.WebMessages;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace ElympicsPlayPad.Tests
{
    public class WebRequestRoutingTests
    {
        private const string Json = "{\"type\":\"testType\",\"message\":\"payload\"}";
        private GameObject _gameObject = null!;

        [SetUp]
        public void SetUp() => _gameObject = new GameObject(nameof(WebRequestRoutingTests));

        [TearDown]
        public void TearDown() => UnityEngine.Object.DestroyImmediate(_gameObject);

        [Test]
        public void JsWebRequestIsRaisedAsWebRequest()
        {
            var communicator = _gameObject.AddComponent<JsCommunicator>();
            string? webRequest = null;
            string? webMessage = null;
            communicator.WebRequestMessageReceived += json => webRequest = json;
            communicator.WebMessageReceived += json => webMessage = json;

            communicator.HandleWebRequest(Json);

            Assert.That(webRequest, Is.EqualTo(Json));
            Assert.That(webMessage, Is.Null);
        }

        [Test]
        public void WebRequestReachesTheReceiversOfItsType()
        {
            var communicator = Substitute.For<IPlayPadCommunicator>();
            var messagingSystem = _gameObject.AddComponent<PlayPadMessagingSystem>();
            messagingSystem.Init(communicator);
            var receiver = Substitute.For<IWebMessageReceiver>();
            messagingSystem.RegisterIWebEventReceiver(receiver, "testType");

            communicator.WebRequestMessageReceived += Raise.Event<Action<string>>(Json);

            receiver.Received(1).OnWebMessage(Arg.Is<WebMessage>(message => message.type == "testType" && message.message == "payload"));
        }
    }
}
