using System.Threading;
using Cysharp.Threading.Tasks;
using SmartContract = ElympicsPlayPad.Web3.Data.SmartContract;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.ContractOperations
{
    public interface IExternalContractOperations
    {
        public UniTask<string> GetValue<T>(SmartContract contract, string name, CancellationToken ct = default, params string[] parameters);
        public UniTask<string> GetFunctionCallData(SmartContract contract, string functionName, CancellationToken ct = default, params object[] parameters);
    }
}
