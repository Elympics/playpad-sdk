using System;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Web
{
    public class StandaloneWebCommunicator: IExternalWebCommunicator
    {
        public void OpenUrl(Uri url) => Application.OpenURL(url.ToString());
    }
}
