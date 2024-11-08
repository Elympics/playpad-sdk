using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Ui
{
    public abstract class CustomStandaloneExternalUiCommunicatorBase : MonoBehaviour, IExternalUiCommunicator
    {
        public abstract UniTask Display(string name, CancellationToken ct = default);
    }
}
