#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.DTO;
using ElympicsPlayPad.ExternalCommunicators.Web3.ContractOperations;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using Nethereum.ABI.EIP712;
using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using UnityEngine;
using SmartContract = ElympicsPlayPad.Web3.Data.SmartContract;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.Wallet
{
    public class StandaloneWalletCommunicator : IExternalWalletCommunicator, IExternalContractOperations
    {
        public event Action<string, string>? WalletConnected;
        public event Action? WalletDisconnected;

        private EthECKey _key;
        private IWeb3 _web3;
        private Account _account;
        private Eip712TypedDataSigner _signer;
        private Dictionary<string, Contract> _contracts;
        private readonly StandaloneWalletConfig _config;

        private static readonly Dictionary<BigInteger, string> PublicRpc = new()
        {
            { 1, "https://ethereum.publicnode.com" },
            { 5, "https://ethereum-goerli.publicnode.com" },
            { 11155111, "https://ethereum-sepolia-rpc.publicnode.com" },
            { 8453, "https://base-rpc.publicnode.com" },
            { 84532, "https://base-sepolia-rpc.publicnode.com"}
        };

        public StandaloneWalletCommunicator(StandaloneWalletConfig standaloneBrowserJsConfig)
        {
            _config = standaloneBrowserJsConfig;
        }

        public async UniTask<ConnectionResponse> Connect()
        {
            if (_config.IsWalletAvailable is false)
                throw new ResponseException(RequestErrors.AddressNotFound, RequestErrors.GetErrorMessage(RequestErrors.AddressNotFound, "Connect"));

            await UniTask.Delay(_config.ConnectionDelay);
            if (!_config.AcceptsConnection)
                throw new Exception("User declined the connection");
            var canParse = BigInteger.TryParse(_config.ChainId,out var chainBigInt);
            if (canParse is false)
                throw new ArgumentException($"Cannot parse chainId provided in {nameof(StandaloneWalletConfig)}");
            InitializeWeb3IfNeeded(chainBigInt);
            return new ConnectionResponse()
            {
                address = _account.Address,
                chainId = _config.ChainId
            };
        }

        public async UniTask<string> SignMessage<TInput>(string address, TInput message)
        {
            await UniTask.Delay(_config.SignatureDelay);
            if (!_config.ShouldConfirmSignatures)
            {
                var reason = "ERROR:Signature Rejected by User";
                throw new Exception(reason);
            }

            var json = typeof(TInput) == typeof(string) ? (string)(object)message : JsonUtility.ToJson(message);
            var signature = _signer.SignTypedDataV4<Domain>(json, _key);
            return signature;
        }

        public void ExternalShowChainSelection()
        {
            Debug.Log("Show Chain Selection Window.");
        }
        public void ExternalShowConnectToWallet()
        {
            Debug.Log("Show Connect To Wallet Window.");
        }
        public void ExternalShowAccountInfo()
        {
            Debug.Log("Show Account Info Window.");
        }
        void IExternalWalletCommunicator.SetPlayPadEventListener(IPlayPadEventListener listener)
        {
            Debug.Log("Set PlayPad listener.");
        }

        private async UniTask<string> SendSignedTransaction(string signedData)
        {
            if (!_config.ShouldSendTransactions)
                throw new Exception("ERROR:Transaction sending failed");
            if (!signedData.HasHexPrefix())
                signedData = "0x" + signedData;
            var transaction = await _web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedData);
            var receipt = await _web3.TransactionReceiptPolling.PollForReceiptAsync(transaction);
            if (receipt.HasErrors() == true)
                throw new Exception($"ERROR:Transaction sending fail status {receipt.Status}, tx: {receipt.TransactionHash}");
            return receipt.TransactionHash;
        }

        private async UniTask<string> SignTransaction(string to, string from, string data)
        {
            if (!_config.ShouldSignTransactions)
                throw new Exception("ERROR:Transaction rejected by user");
            var input = new TransactionInput { To = to, From = from, Data = data };
            input.Gas = await _web3.TransactionManager.EstimateGasAsync(input);
            return await _web3.TransactionManager.SignTransactionAsync(input);
        }

        public async UniTask<string> SendTransaction(string to, string from, string data)
        {
            var signedData = await SignTransaction(to, from, data);
            await UniTask.Yield();
            return await SendSignedTransaction(signedData);
        }

        public async UniTask<string> GetValue<T>(SmartContract contract, string name, params string[] parameters)
        {
            if (!_config.ShouldGetValues)
            {
                var reason = "ERROR:Does not work";
                return reason.ToString();
            }
            var nethereumContract = GetOrCreateContract(contract);
            var decimalsFunction = nethereumContract.GetFunction(name);

            var par = parameters.Select(x => (object)x).ToArray();

            T result;
            if (parameters.Any())
                result = await decimalsFunction.CallAsync<T>(par);
            else
                result = await decimalsFunction.CallAsync<T>();
            return result.ToString();
        }

        public UniTask<string> GetFunctionCallData(SmartContract contract, string functionName, params object[] parameters)
        {
            var nethContract = GetOrCreateContract(contract);
            var nethFunction = nethContract.GetFunction(functionName);
            return UniTask.FromResult(nethFunction.GetData(parameters));
        }

        private void InitializeWeb3IfNeeded(BigInteger chainId)
        {
            if (_key is null
                || _key.GetPrivateKey() != _config.GetWalletKey().GetPrivateKey())
                _account = null;
            if (_account != null)
                return;
            try
            {
                _key = _config.GetWalletKey();
                _account = new Account(_key, chainId);
                Debug.Log($"Local Eth account {_account.Address}\n Use this private key to top up metamask:\n{_account.PrivateKey}\n");
                _web3 = new Nethereum.Web3.Web3(_account, PublicRpc[chainId]);
                // MetamaskHostProvider host = MetamaskWebglHostProvider.CreateOrGetCurrentInstance();
                // _web3 = await host.GetWeb3Async();
                _signer = new Eip712TypedDataSigner();
                _contracts = new Dictionary<string, Contract>();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to initialize web3 account. Check if the private key is valid.\n{e}");
            }
        }

        private Contract GetOrCreateContract(SmartContract contract)
        {
            if (string.IsNullOrEmpty(contract.Address))
                throw new Exception("Contract not initialized!!!");
            var address = contract.Address;
            if (_contracts.TryGetValue(address, out var createContract))
                return createContract;

            var nethereumContract = _web3.Eth.GetContract(contract.ABI, address);
            _contracts[address] = nethereumContract;
            return nethereumContract;
        }

        public void Dispose()
        { }
    }
}
