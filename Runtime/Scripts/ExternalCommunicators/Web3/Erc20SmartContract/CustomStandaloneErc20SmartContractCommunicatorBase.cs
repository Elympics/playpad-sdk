using System.Numerics;
using System.Threading;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.Web3.Data;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.Erc20SmartContract
{
    public abstract class CustomStandaloneErc20SmartContractCommunicatorBase : MonoBehaviour, IExternalERC20SmartContractOperations
    {
        public abstract UniTask<string> GetDecimals(SmartContract tokenContract, CancellationToken ct = default);
        public abstract UniTask<string> GetBalance(SmartContract tokenContract, string owner, CancellationToken ct = default);
        public abstract UniTask<string> GetSymbol(SmartContract tokenContract, CancellationToken ct = default);
        public abstract UniTask<string> GetName(SmartContract tokenContract, CancellationToken ct = default);
        public abstract UniTask<string> GetAllowance(SmartContract tokenContract, string owner, string spender, CancellationToken ct = default);
        public abstract UniTask<string> Approve(SmartContract tokenContract, string owner, string spender, BigInteger value, CancellationToken ct = default);
    }
}
