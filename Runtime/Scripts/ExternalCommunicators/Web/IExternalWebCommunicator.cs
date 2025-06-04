using System;
namespace ElympicsPlayPad.ExternalCommunicators.Web
{
    public interface IExternalWebCommunicator
    {
        void OpenUrl(Uri uri);
    }
}
