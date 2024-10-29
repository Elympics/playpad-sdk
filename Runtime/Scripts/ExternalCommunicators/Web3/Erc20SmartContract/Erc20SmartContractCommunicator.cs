using System.Numerics;
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

        public async UniTask<string> GetDecimals(SmartContract tokenContract) => await _externalContractOperations.GetValue<int>(tokenContract, ValueCalls.Decimals);

        public async UniTask<string> GetBalance(SmartContract tokenContract, string owner) => await _externalContractOperations.GetValue<BigInteger>(tokenContract, ValueCalls.BalanceOf, owner);

        public async UniTask<string> GetSymbol(SmartContract tokenContract) => await _externalContractOperations.GetValue<string>(tokenContract, ValueCalls.Symbol);

        public async UniTask<string> GetName(SmartContract tokenContract) => await _externalContractOperations.GetValue<string>(tokenContract, ValueCalls.Name);

        public async UniTask<string> GetAllowance(SmartContract tokenContract, string owner, string spender) => await _externalContractOperations.GetValue<BigInteger>(tokenContract, ValueCalls.Allowance, owner, spender);

        async UniTask<string> IExternalERC20SmartContractOperations.Approve(SmartContract tokenContract, string owner, string spender, BigInteger value)
        {
            var parameters = new[]
            {
                spender,
                value.ToString()
            };

            var data = await _externalContractOperations.GetFunctionCallData(tokenContract, EncodeFunctionDataCallsERC20.Approve, parameters);
            return await _externalWalletOperator.SendTransaction(tokenContract.Address, owner, data);
        }
    }
}
