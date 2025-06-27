#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Requests;
using ElympicsPlayPad.Protocol.Responses;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.Wallet
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class WebGLExternalWalletCommunicator : IExternalWalletCommunicator
    {
        private readonly JsCommunicator _communicator;
        public WebGLExternalWalletCommunicator(JsCommunicator jsCommunicator) => _communicator = jsCommunicator;

        public async UniTask<string> SignMessage(string address, string message, CancellationToken ct = default)
        {
            var payload = new SignTypedDataRequest
            {
                address = address,
                dataToSign = message,
            };

            var result = await _communicator.SendRequestMessage<SignTypedDataRequest, StringPayloadResponse>(RequestResponseMessageTypes.SignTypedData, payload, ct);
            return result.message;
        }

        public async UniTask<string> SendTransaction(string to, string from, string data, CancellationToken ct = default)
        {
            var transaction = new TransactionToSignRequest()
            {
                to = to,
                from = from,
                data = data,
            };
            var result = await _communicator.SendRequestMessage<TransactionToSignRequest, StringPayloadResponse>(RequestResponseMessageTypes.SendTransaction, transaction, ct);
            return result.message;
        }

        public void Dispose()
        { }
    }
}
