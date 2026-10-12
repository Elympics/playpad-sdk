using System.Threading;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Requests;

namespace ElympicsPlayPad.ExternalCommunicators.Ui
{
    internal class WebGLExternalUiCommunicator : IExternalUiCommunicator
    {
        private readonly PlayPadMessagingSystem _playPadMessagingSystem;

        public WebGLExternalUiCommunicator(PlayPadMessagingSystem playPadMessagingSystem) => _playPadMessagingSystem = playPadMessagingSystem;

        public async UniTask Display(string name, string payload = null) =>
            _ = await _playPadMessagingSystem.SendRequestMessage<ShowPlayPadModalRequest, EmptyPayload>(RequestResponseMessageTypes.ShowPlayPadModal,
                new ShowPlayPadModalRequest { modalName = name, payload = payload }, CancellationToken.None);
    }
}
