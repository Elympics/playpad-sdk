using System.Numerics;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.Web3.Data;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.Erc20SmartContract
{
    public abstract class CustomStandaloneErc20SmartContractCommunicatorBase : MonoBehaviour, IExternalERC20SmartContractOperations
    {
        public abstract UniTask<string> GetDecimals(SmartContract tokenContract);
        public abstract UniTask<string> GetBalance(SmartContract tokenContract, string owner);
        public abstract UniTask<string> GetSymbol(SmartContract tokenContract);
        public abstract UniTask<string> GetName(SmartContract tokenContract);
        public abstract UniTask<string> GetAllowance(SmartContract tokenContract, string owner, string spender);
        public abstract UniTask<string> Approve(SmartContract tokenContract, string owner, string spender, BigInteger value);
    }
}
