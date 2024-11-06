using System;
using ElympicsPlayPad.Protocol.WebMessages;

namespace ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js
{
    internal interface IJsCommunicatorRetriever
    {
        public event Action<string> ResponseObjectReceived;
        public event Action<WebMessage> WebObjectReceived;
    }
}
