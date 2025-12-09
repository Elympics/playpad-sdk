using System;
using Cysharp.Threading.Tasks;
namespace ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js
{
    internal interface IPlayPadCommunicator : IDisposable
    {
        public event Action<string> ResponseMessageReceived;
        public event Action<string> WebMessageReceived;
        public void SendRequestMessage(string messageType, string jsonMessage);
        UniTask Connect();
    }
}
