using System.Threading;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.Web3.Data;
using Nethereum.ABI;
using BigInteger = System.Numerics.BigInteger;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.Erc20SmartContract
{
    public interface IExternalERC20SmartContractOperations
    {
        public UniTask<string> GetDecimals(SmartContract tokenContract, CancellationToken ct = default);
        public UniTask<string> GetBalance(SmartContract tokenContract, string owner, CancellationToken ct = default);
        public UniTask<string> GetSymbol(SmartContract tokenContract, CancellationToken ct = default);
        public UniTask<string> GetName(SmartContract tokenContract, CancellationToken ct = default);
        public UniTask<string> GetAllowance(SmartContract tokenContract, string owner, string spender, CancellationToken ct = default);
        public UniTask<string> ApproveMax(SmartContract tokenContract, string owner, string spender, CancellationToken ct = default) => Approve(tokenContract, owner, spender, IntType.MAX_UINT256_VALUE, ct);
        public UniTask<string> Approve(SmartContract tokenContract, string owner, string spender, BigInteger value, CancellationToken ct = default);
    }
}