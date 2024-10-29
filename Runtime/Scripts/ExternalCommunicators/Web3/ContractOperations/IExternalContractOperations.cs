using Cysharp.Threading.Tasks;
using SmartContract = ElympicsPlayPad.Web3.Data.SmartContract;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.ContractOperations
{
    public interface IExternalContractOperations
    {
        public UniTask<string> GetValue<T>(SmartContract contract, string name, params string[] parameters);
        public UniTask<string> GetFunctionCallData(SmartContract contract, string functionName, params object[] parameters);
    }
}
