using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Ui
{
    public class StandaloneExternalUiCommunicator : CustomStandaloneExternalUiCommunicatorBase
    {
        public override UniTask Display(string name, string payload = null)
        {
            Debug.Log($"Show PlayPad UI: \"{name}\", payload: \"{payload}\"");
            return UniTask.CompletedTask;
        }
    }
}
