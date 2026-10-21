using System;
using System.Threading;
using Elympics.Cysharp.Threading.Tasks;

namespace ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js
{
    public interface IPlayPadMessagingSystem
    {
        /// <param name="timeout">
        /// Per-request override of the dispatcher's default wait. Pass one only for
        /// machine-to-machine requests that should fail fast — requests that wait on a
        /// person (wallet signatures, on-ramp, modals) can legitimately take minutes
        /// and must keep the default.
        /// </param>
        UniTask<TReturn> SendRequestMessage<TInput, TReturn>(string messageType, TInput? payload, CancellationToken ct, TimeSpan? timeout = null)
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
