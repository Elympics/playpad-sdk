using System.Threading;
using Cysharp.Threading.Tasks;

namespace ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js
{
    public interface IPlayPadMessagingSystem
    {
        UniTask<TReturn> SendRequestMessage<TInput, TReturn>(string messageType, TInput? payload, CancellationToken ct)
            where TInput : struct
            where TReturn : struct;

        void SendVoidMessage<TInput>(string messageType, TInput? payload = null)
            where TInput : struct;

        void RegisterIWebEventReceiver(IWebMessageReceiver receiver, string messageType);
        void RegisterIWebEventReceiver(IWebMessageReceiver receiver, params string[] messageTypes);

        void UnregisterIWebEventReceiver(IWebMessageReceiver receiver, string messageType);

        UniTask Connect();
    }

}
