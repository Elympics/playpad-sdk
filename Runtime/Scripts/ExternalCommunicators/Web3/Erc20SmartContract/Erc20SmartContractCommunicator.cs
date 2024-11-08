using System.Numerics;
using System.Threading;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.ExternalCommunicators.Web3.ContractOperations;
using ElympicsPlayPad.ExternalCommunicators.Web3.Wallet;
using ElympicsPlayPad.Web3.Utility;
using SmartContract = ElympicsPlayPad.Web3.Data.SmartContract;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.Erc20SmartContract
{
    public class Erc20SmartContractCommunicator : IExternalERC20SmartContractOperations
    {
        private readonly IExternalContractOperations _externalContractOperations;
        private readonly IExternalWalletOperations _externalWalletOperator;
        public Erc20SmartContractCommunicator(IExternalContractOperations externalContractOperations, IExternalWalletOperations externalWalletOperator)
        {
            _externalContractOperations = externalContractOperations;
            _externalWalletOperator = externalWalletOperator;
        }

        public async UniTask<string> GetDecimals(SmartContract tokenContract, CancellationToken ct = default) => await _externalContractOperations.GetValue<int>(tokenContract, ValueCalls.Decimals, ct);

        public async UniTask<string> GetBalance(SmartContract tokenContract, string owner, CancellationToken ct = default) => await _externalContractOperations.GetValue<BigInteger>(tokenContract, ValueCalls.BalanceOf, ct, owner);

        public async UniTask<string> GetSymbol(SmartContract tokenContract, CancellationToken ct = default) => await _externalContractOperations.GetValue<string>(tokenContract, ValueCalls.Symbol, ct);

        public async UniTask<string> GetName(SmartContract tokenContract, CancellationToken ct = default) => await _externalContractOperations.GetValue<string>(tokenContract, ValueCalls.Name, ct);

        public async UniTask<string> GetAllowance(SmartContract tokenContract, string owner, string spender, CancellationToken ct = default) => await _externalContractOperations.GetValue<BigInteger>(tokenContract, ValueCalls.Allowance, ct, owner, spender);

        async UniTask<string> IExternalERC20SmartContractOperations.Approve(SmartContract tokenContract, string owner, string spender, BigInteger value, CancellationToken ct)
        {
            var parameters = new[]
            {
                spender,
                value.ToString()
            };

            var data = await _externalContractOperations.GetFunctionCallData(tokenContract, EncodeFunctionDataCallsERC20.Approve, ct, parameters).SuppressCancellationThrow();
            if (data.IsCanceled)
                return string.Empty;

            return await _externalWalletOperator.SendTransaction(tokenContract.Address, owner, data.Result, ct);
        }
    }
}
