using ElympicsPlayPad.DTO;

namespace ElympicsPlayPad.ExternalCommunicators.WebCommunication
{
    public interface IWebMessageReceiver
    {
        void OnWebMessage(WebMessageObject message);
    }
}
