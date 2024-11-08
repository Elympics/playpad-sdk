#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.ExternalCommunicators.Web3.ContractOperations;
using Nethereum.Contracts;
using SmartContract = ElympicsPlayPad.Web3.Data.SmartContract;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.Wallet
{
    public class StandaloneWalletCommunicator : IExternalWalletCommunicator, IExternalContractOperations
    {
        private static readonly Dictionary<string, string> PublicRpc = new()
        {
            { "1", "https://ethereum.publicnode.com" },
            { "5", "https://ethereum-goerli.publicnode.com" },
            { "11155111", "https://ethereum-sepolia-rpc.publicnode.com" },
            { "8453", "https://base-rpc.publicnode.com" },
            { "84532", "https://base-sepolia-rpc.publicnode.com" }
        };

        private readonly Dictionary<string, Contract> _contracts = new();

        public async UniTask<string> GetValue<T>(SmartContract contract, string name, CancellationToken ct = default, params string[] parameters)
        {
            var nethereumContract = GetOrCreateContract(contract);
            var decimalsFunction = nethereumContract.GetFunction(name);
            var par = parameters.Select(x => (object)x).ToArray();

            var result = parameters.Any() ? await decimalsFunction.CallAsync<T>(par) : await decimalsFunction.CallAsync<T>();
            return result?.ToString() ?? string.Empty;
        }

        private Contract GetOrCreateContract(SmartContract contract)
        {
            if (string.IsNullOrEmpty(contract.Address)
                || string.IsNullOrEmpty(contract.ChainId))
                throw new Exception("Contract not initialized!!!");
            var address = contract.Address;

            if (_contracts.TryGetValue(address, out var createContract))
                return createContract;

            var web3 = new Nethereum.Web3.Web3(PublicRpc[contract.ChainId]);
            var nethereum = web3.Eth.GetContract(contract.ABI, address);

            _contracts[address] = nethereum;
            return nethereum;
        }

        public void Dispose()
        { }

        //TODO: delete
        public UniTask<string> SignMessage(string address, string message, CancellationToken ct = default)
        {
            UnityEngine.Debug.LogError($"{nameof(SignMessage)} is unavailable in {nameof(StandaloneWalletCommunicator)}");
            return UniTask.FromResult(string.Empty);
        }
        public UniTask<string> SendTransaction(string to, string from, string data, CancellationToken ct = default)
        {
            UnityEngine.Debug.LogError($"{nameof(SendTransaction)} is unavailable in {nameof(StandaloneWalletCommunicator)}");
            return UniTask.FromResult(string.Empty);
        }
        public UniTask<string> GetFunctionCallData(SmartContract contract, string functionName, CancellationToken ct = default, params object[] parameters)
        {
            UnityEngine.Debug.LogError($"{nameof(GetFunctionCallData)} is unavailable in {nameof(StandaloneWalletCommunicator)}");
            return UniTask.FromResult(string.Empty);
        }
    }
}
