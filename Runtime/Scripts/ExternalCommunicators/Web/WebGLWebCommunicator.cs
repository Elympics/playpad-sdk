using System;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.VoidMessages;

namespace ElympicsPlayPad.ExternalCommunicators.Web
{
    internal class WebGLWebCommunicator : IExternalWebCommunicator
    {
        private readonly PlayPadMessagingSystem _playPadMessagingSystem;
        public WebGLWebCommunicator(PlayPadMessagingSystem playPadMessagingSystem) => _playPadMessagingSystem = playPadMessagingSystem;

        public void OpenUrl(Uri uri)
        {
            if (uri is null)
                throw new ArgumentNullException(nameof(uri));
            if (!uri.IsAbsoluteUri)
                throw new ArgumentException("The provided url must be absolute.", nameof(uri));

            var message = new OpenUrlMessage
            {
                url = uri.AbsoluteUri,
            };

            _playPadMessagingSystem.SendVoidMessage<OpenUrlMessage>(VoidMessageTypes.OpenUrlMessage, message);
        }
    }
}
