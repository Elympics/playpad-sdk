#nullable enable
using System;
using Elympics.Cysharp.Threading.Tasks;

namespace ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js
{
    internal interface IPlayPadCommunicator : IDisposable
    {
        event Action<string>? ResponseMessageReceived;
        event Action<string>? WebMessageReceived;
        event Action<string>? WebRequestMessageReceived;
        void SendRequestMessage(string messageType, string jsonMessage);
        UniTask Connect();
    }
}
