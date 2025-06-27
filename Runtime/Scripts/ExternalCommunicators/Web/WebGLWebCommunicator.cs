using System;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.VoidMessages;
namespace ElympicsPlayPad.ExternalCommunicators.Web
{
    internal class WebGLWebCommunicator : IExternalWebCommunicator
    {
        private readonly JsCommunicator _jsCommunicator;
        public WebGLWebCommunicator(JsCommunicator jsCommunicator) => _jsCommunicator = jsCommunicator;

        public void OpenUrl(Uri uri)
        {
            if (uri is null)
                throw new ArgumentNullException(nameof(uri));
            if (!uri.IsAbsoluteUri)
                throw new ArgumentException("The provided url must be absolute.", nameof(uri));
            if (!uri.IsWellFormedOriginalString())
                throw new ArgumentException("The uri is not well-formed.", nameof(uri));

            var message = new OpenUrlMessage()
            {
                url = uri.ToString(),
            };

            _jsCommunicator.SendVoidMessage<OpenUrlMessage>(VoidMessageTypes.OpenUrlMessage, message);
        }
    }
}
