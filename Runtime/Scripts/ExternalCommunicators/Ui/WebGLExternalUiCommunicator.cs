using Cysharp.Threading.Tasks;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Requests;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Ui
{
    internal class WebGLExternalUiCommunicator : IExternalUiCommunicator
    {
        private readonly JsCommunicator _communicator;

        public WebGLExternalUiCommunicator(JsCommunicator communicator)
        {
            _communicator = communicator;
        }

        public async UniTask Display(string name)
        {
            Debug.Log($"[{nameof(IExternalUiCommunicator)}] Displaying {name} modal started");

            _ = await _communicator.SendRequestMessage<ShowPlayPadModalRequest, EmptyPayload>(ReturnEventTypes.ShowPlayPadModal, new ShowPlayPadModalRequest { modalName = name });

            Debug.Log($"[{nameof(IExternalUiCommunicator)}] Displaying {name} modal ended");
        }
    }
}
