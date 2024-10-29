#nullable enable

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using ElympicsPlayPad.DTO;
using ElympicsPlayPad.ExternalCommunicators;
using ElympicsPlayPad.ExternalCommunicators.Web3.Erc20SmartContract;
using ElympicsPlayPad.ExternalCommunicators.Web3.Trust;
using ElympicsPlayPad.ExternalCommunicators.Web3.Wallet;
using ElympicsPlayPad.Utility;
using ElympicsPlayPad.Web3.Data;
using ElympicsPlayPad.Web3.Extensions;
using ElympicsPlayPad.Wrappers;
using JetBrains.Annotations;
using SCS;
using UnityEngine;
using SmartContract = ElympicsPlayPad.Web3.Data.SmartContract;

namespace ElympicsPlayPad.Web3
{
    [DefaultExecutionOrder(ElympicsLobbyExecutionOrders.Web3Wallet)]
    public class Web3Wallet : ElympicsEthSigner, IWallet
    {
        [PublicAPI]
        public event Action<WalletConnectionStatus>? WalletConnectionUpdated;

        internal event Action<WalletConnectionStatus>? WalletConnectionUpdatedInternal;
        public override string Address => _address;

        private string? _address;
        private ISmartContractServiceWrapper? _scs;

        [PublicAPI]
        public ChainConfig? CurrentChain => ThrowIfScsNull(_scs!.CurrentChain);

        private static IExternalWalletCommunicator WalletCommunicator => PlayPadCommunicator.Instance!.WalletCommunicator!;
        private static IExternalTrustSmartContractOperations TrustCommunicator => PlayPadCommunicator.Instance!.TrustCommunicator!;
        private static IExternalERC20SmartContractOperations Erc20Communicator => PlayPadCommunicator.Instance!.TokenCommunicator!;

        private SmartContract TokenContract => CurrentChain!.Value.GetSmartContract(SmartContractType.ERC20Token).ToSmartContract(CurrentChain.Value.ChainId);

        private void Start()
        {
            _scs = GetComponent<ISmartContractServiceWrapper>();
            if (_scs != null)
                _scs.RegisterWallet(this);
            Subscribe();
        }

        public async UniTask<string> ConnectWeb3()
        {
            var result = await WalletCommunicator.Connect();
            _address = result.address;
            _chainId = BigInteger.Parse(result.chainId);
            return _address;
        }

        public async UniTask<string> SignTypedDataV4(string message)
        {
            Debug.Log($"[Wallet] Sign message {message} using address {Address}");
            return await WalletCommunicator.SignMessage(Address, message);
        }

        public async UniTask<string> SendTransaction(SendTransactionWalletRequest value)
        {
            try
            {
                return await WalletCommunicator.SendTransaction(value.To, value.From, value.Data);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw e;
            }
        }

        public override BigInteger ChainId => _chainId;

        private BigInteger _chainId;
        public override async UniTask<string> SignAsync(string message, CancellationToken ct = default)
        {
            Debug.Log($"[Wallet] Data to sign: {message} using address {Address}");
            return await WalletCommunicator!.SignMessage(Address, message);
        }

        public async UniTask<int> GetDecimals()
        {
            Debug.Log("[Wallet] Getting decimals for token...");
            return ThrowIfScsNull(_scs!.CurrentChain!.Value.Decimals);
        }

        public async UniTask<BigInteger> GetBalance()
        {
            return BigInteger.Parse(await Erc20Communicator.GetBalance(TokenContract, Address));
        }

        public async UniTask<string> GetName()
        {
            return await Erc20Communicator.GetName(TokenContract);
        }

        public async UniTask<string> GetSymbol()
        {
            return await Erc20Communicator.GetSymbol(TokenContract);
        }

        public async UniTask ApproveMax(string spender)
        {
            Debug.Log($"[Wallet] Approving token spend for {spender}...");
            await Erc20Communicator.ApproveMax(TokenContract, Address, spender);
        }

        public void Deposit()
        {
            Debug.Log($"[Wallet] Depositing to Trust...");
            TrustCommunicator.ShowTrustPanel();
        }

        public async UniTask<TrustState> GetDeposit()
        {
            Debug.Log("[Wallet] Get Trust amount");
            return await TrustCommunicator.GetTrustState();
        }

        public async UniTask<BigInteger> GetAllowance(string spender)
        {
            Debug.Log("[Wallet] Getting token allowance...");
            return BigInteger.Parse(await Erc20Communicator.GetAllowance(TokenContract, Address, spender));
        }
        public void ExternalShowChainSelection()
        {
            WalletCommunicator.ExternalShowChainSelection();
        }
        public void ExternalShowConnectToWallet()
        {
            WalletCommunicator.ExternalShowConnectToWallet();
        }
        public void ExternalShowAccountInfo()
        {
            WalletCommunicator.ExternalShowAccountInfo();
        }

        private void OnWalletConnected(string address, string chainId)
        {
            if (_address == address)
                return;

            _address = address;
            _chainId = BigInteger.Parse(chainId);
            WalletConnectionUpdatedInternal?.Invoke(WalletConnectionStatus.Connected);
            WalletConnectionUpdated?.Invoke(WalletConnectionStatus.Connected);

        }
        private void OnWalletDisconnected()
        {
            _address = null;
            _chainId = BigInteger.Zero;
            WalletConnectionUpdatedInternal?.Invoke(WalletConnectionStatus.Disconnected);
            WalletConnectionUpdated?.Invoke(WalletConnectionStatus.Disconnected);
        }

        private void OnDestroy()
        {
            UnSubscribe();
        }

        private void Subscribe()
        {

            PlayPadCommunicator.Instance.WalletConnected += OnWalletConnected;
            PlayPadCommunicator.Instance.WalletDisconnected += OnWalletDisconnected;
        }
        private void UnSubscribe()
        {
            PlayPadCommunicator.Instance.WalletConnected -= OnWalletConnected;
            PlayPadCommunicator.Instance.WalletDisconnected -= OnWalletDisconnected;
        }

        private T ThrowIfScsNull<T>(T val, [CallerMemberName] string methodName = "")
        {
            if (_scs == null)
                throw new NullReferenceException($"Using {methodName} requires {nameof(DefaultSmartContractServiceWrapper)} and {nameof(SmartContractService)} attached to {gameObject.name}.");
            return val;
        }
    }
}
