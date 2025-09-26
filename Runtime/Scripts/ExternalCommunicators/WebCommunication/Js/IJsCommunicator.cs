using System.Threading;
using Cysharp.Threading.Tasks;

#nullable enable

namespace ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js
{
    internal interface IJsCommunicator : IJsCommunicatorRetriever
    {
        public UniTask<TReturn> SendRequestMessage<TInput, TReturn>(string messageType, TInput? payload, CancellationToken ct)
            where TInput : struct
            where TReturn : struct;

        public void SendVoidMessage<TInput>(string messageType, TInput? payload = null)
            where TInput : struct;

        public void RegisterIWebEventReceiver(IWebMessageReceiver receiver, string messageType);
        public void RegisterIWebEventReceiver(IWebMessageReceiver receiver, params string[] messageTypes);
    }
}
