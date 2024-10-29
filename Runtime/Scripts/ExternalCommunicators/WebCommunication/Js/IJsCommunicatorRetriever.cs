using System;
using ElympicsPlayPad.DTO;

namespace ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js
{
    internal interface IJsCommunicatorRetriever
    {
        public event Action<string> ResponseObjectReceived;
        public event Action<WebMessageObject> WebObjectReceived;
    }
}
