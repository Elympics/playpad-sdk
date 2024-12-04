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
        public async UniTask Display(string name, CancellationToken ct = default)
        {
            Debug.Log($"Show PlayPad UI: \"{name}\"");
            await UniTask.CompletedTask;
        }
    }
}
