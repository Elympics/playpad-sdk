using System;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Responses;
using ElympicsPlayPad.Protocol.WebMessages;
using UnityEngine;

namespace ElympicsPlayPad.Tests.PlayMode.Mocks
{
    public class JsCommunicatorRetrieverMock : IJsCommunicatorRetriever
    {
        public event Action<string> ResponseObjectReceived;
        public event Action<WebMessage> WebObjectReceived;


        public void SendHandshakeResponse(int ticket, int status)
        {
            var handshakeResponse = GetHandshakeResponse();
            var response = new ResponseMessage
            {
                ticket = ticket,
                type = ReturnEventTypes.Handshake,
                status = status,
                response = JsonUtility.ToJson(handshakeResponse)
            };

            var json = JsonUtility.ToJson(response);

            ResponseObjectReceived?.Invoke(json);
        }


        private HandshakeResponse GetHandshakeResponse() => new HandshakeResponse
        {
            error = null,
            device = "mobile",
            environment = "PROD",
            capabilities = 3,
            featureAccess = 5,
            closestRegion = "mumbai"
        };
    }
}
