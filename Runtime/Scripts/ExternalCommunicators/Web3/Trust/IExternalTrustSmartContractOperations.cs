using Cysharp.Threading.Tasks;
using ElympicsPlayPad.DTO;
using ElympicsPlayPad.Web3.Data;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.Trust
{
    public interface IExternalTrustSmartContractOperations
    {
        public void ShowTrustPanel();

        public UniTask<TrustState> GetTrustState();
    }
}
