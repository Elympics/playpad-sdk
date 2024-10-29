using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Ui
{
    public class StandaloneExternalUiCommunicator : IExternalUiCommunicator
    {
        public async UniTask Display(string name)
        {
            Debug.Log($"[{nameof(IExternalUiCommunicator)}] Displayed {name} modal");

            await UniTask.CompletedTask;
        }
    }
}
