#nullable enable
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.DTO;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Session.Exceptions;
using ElympicsPlayPad.Wrappers;
using UnityEngine;
using TransactionToSign = ElympicsPlayPad.DTO.TransactionToSign;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.Wallet
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class WebGLExternalWalletCommunicator : IExternalWalletCommunicator
    {
        private readonly JsCommunicator _communicator;
        private readonly ISmartContractServiceWrapper? _smartContractService;
        private IPlayPadEventListener _connectionListener = null!;
        public WebGLExternalWalletCommunicator(JsCommunicator jsCommunicator, ISmartContractServiceWrapper? smartContractService)
        {
            _smartContractService = smartContractService;
            _communicator = jsCommunicator;
            _communicator.WebObjectReceived += OnWebObjectReceived;
            _smartContractService = smartContractService;
        }

        private void OnWebObjectReceived(WebMessageObject messageObject)
        {
            switch (messageObject.type)
            {
                case WebMessageTypes.TrustTransactionFinished:
                    OnTrustOperationFinished(messageObject.message);
                    break;
            }
        }

        //TODO: usun to stad do TrustCommunicatora.
        private void OnTrustOperationFinished(string webMessageMessage)
        {
            var trustDeposit = JsonUtility.FromJson<TrustDepositTransactionFinishedWebMessage>(webMessageMessage);
            _connectionListener.OnTrustDepositFinished(trustDeposit);
        }
        private void OnWalletConnected(string webMessageMessage)
        {
            var walletConnectedData = JsonUtility.FromJson<WalletConnectionWebMessage>(webMessageMessage);
            if (walletConnectedData.isConnected)
                _connectionListener.OnWalletConnected(walletConnectedData.address, walletConnectedData.chainId);
            else
                _connectionListener.OnWalletDisconnected();
        }

        public async UniTask<string> SignMessage<TInput>(string address, TInput message) => await _communicator.SendRequestMessage<TInput, string>(ReturnEventTypes.SignTypedData, message, address);

        public async UniTask<ConnectionResponse> Connect()
        {
            var response = await _communicator.SendRequestMessage<string, ConnectionResponse>(ReturnEventTypes.Connect, string.Empty);
            if (_smartContractService == null)
                return response;

            if (response.chainId != _smartContractService.CurrentChain!.Value.ChainId)
                throw new ChainIdMismatch(response.chainId, _smartContractService.CurrentChain!.Value.ChainId);
            return response;
        }

        public async UniTask<string> SendTransaction(string to, string from, string data)
        {
            var transaction = new TransactionToSign()
            {
                to = to,
                from = from,
                data = data,
            };
            return await _communicator.SendRequestMessage<TransactionToSign, string>(ReturnEventTypes.SendTransaction, transaction);
        }
        void IExternalWalletCommunicator.SetPlayPadEventListener(IPlayPadEventListener listener) => _connectionListener = listener;


        public void ExternalShowChainSelection() => _communicator.SendVoidMessage(VoidEventTypes.ShowChainSelectionUI, string.Empty);
        public void ExternalShowConnectToWallet() => _communicator.SendVoidMessage(VoidEventTypes.ShowConnectToWallet, string.Empty);
        public void ExternalShowAccountInfo() => _communicator.SendVoidMessage(VoidEventTypes.ShowAccountUI, string.Empty);
        public void Dispose()
        {
            Debug.Log($"[{nameof(WebGLExternalWalletCommunicator)}] Dispose.");
            _communicator.WebObjectReceived -= OnWebObjectReceived;
        }
    }
}
