using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Ui
{
    public class StandaloneExternalUiCommunicator : IExternalUiCommunicator
    {
        public async UniTask Display(string name)
        {
            Debug.Log($"Show PlayPad UI: \"{name}\"");
            await UniTask.CompletedTask;
        }
    }
}
