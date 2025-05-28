using System;
using ElympicsPlayPad.Protocol.WebMessages;

namespace ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js
{
    internal interface IJsCommunicatorRetriever
    {
        event Action<string> ResponseObjectReceived;
        event Action<WebMessage> WebObjectReceived;
    }
}
