using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Ui
{
    public class StandaloneExternalUiCommunicator : CustomStandaloneExternalUiCommunicatorBase
    {
        public override UniTask Display(string name)
        {
            Debug.Log($"Show PlayPad UI: \"{name}\"");
            return UniTask.CompletedTask;
        }
    }
}
