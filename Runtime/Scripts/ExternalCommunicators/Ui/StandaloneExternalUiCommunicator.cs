using System.Threading;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Requests;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Ui
{
    public class StandaloneExternalUiCommunicator : IExternalUiCommunicator
    {
        private bool _release;
        public async UniTask Display(string name, CancellationToken ct = default)
        {
            var _communicator = MonoBehaviour.FindObjectOfType<JsCommunicator>();
            _ = await _communicator.SendRequestMessage<ShowPlayPadModalRequest, EmptyPayload>(ReturnEventTypes.ShowPlayPadModal, new ShowPlayPadModalRequest { modalName = name }, ct);

        }
    }
}
