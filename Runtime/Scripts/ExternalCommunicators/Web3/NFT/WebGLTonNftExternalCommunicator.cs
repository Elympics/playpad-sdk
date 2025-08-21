#nullable enable

using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Requests;
using ElympicsPlayPad.Protocol.Responses;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.NFT
{
    internal class WebGLTonNftExternalCommunicator : ITonNftExternalCommunicator
    {
        private readonly JsCommunicator _jsCommunicator;

        internal WebGLTonNftExternalCommunicator(JsCommunicator jsCommunicator) => _jsCommunicator = jsCommunicator;

        public async UniTask<bool> MintNft(string collectionAddress, string price, string payload, CancellationToken ct = default)
        {
            var response = await _jsCommunicator.SendRequestMessage<MintNftRequest<MintTonNftPayload>, BoolPayloadResponse>(RequestResponseMessageTypes.MintNft,
                new MintNftRequest<MintTonNftPayload>
                {
                    collectionAddress = collectionAddress,
                    price = price,
                    type = ChainTypes.Ton,
                    payload = new MintTonNftPayload { payload = payload }
                },
                ct);

            return response.result;
        }
        public async UniTask<string> SendRawTransaction(string address, string amount, string payload, string? stateInit, CancellationToken ct = default)
        {
            var request = new SendRawTransactionRequest<TonPayload>
            {
                type = ChainTypes.Ton,
                destinationAddress = address,
                amount = amount,
                payload = new TonPayload
                {
                    stateInit = stateInit,
                    payload = payload
                }
            };

            var response = await _jsCommunicator.SendRequestMessage<SendRawTransactionRequest<TonPayload>, SendRawTransactionResponse>(RequestResponseMessageTypes.SendRawTransaction, request, ct);
            if (!string.IsNullOrEmpty(response.error))
                throw new ElympicsException("Couldn't send transaction: " + response.error);
            return response.txHash;
        }
    }
}
