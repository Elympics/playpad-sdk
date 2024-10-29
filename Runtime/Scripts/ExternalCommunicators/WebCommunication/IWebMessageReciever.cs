using ElympicsPlayPad.Protocol.WebMessages;

namespace ElympicsPlayPad.ExternalCommunicators.WebCommunication
{
    public interface IWebMessageReceiver
    {
        void OnWebMessage(WebMessage message);
    }
}
