using System.Numerics;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Requests;
using ElympicsPlayPad.Protocol.Responses;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.NFT
{
    internal class WebGLEvmExternalCommunicator : IEvmExternalCommunicator
    {
        private readonly JsCommunicator _jsCommunicator;

        public WebGLEvmExternalCommunicator(JsCommunicator jsCommunicator) => _jsCommunicator = jsCommunicator;

        public async UniTask<bool> MintNft(string collectionAddress, string price, BigInteger chainId, string data, CancellationToken ct = default)
        {
            var payload = new MintNftRequest<MintEvmNftPayload>
            {
                collectionAddress = collectionAddress,
                price = price,
                type = ChainTypes.Evm,
                payload = new MintEvmNftPayload
                {
                    chainId = chainId.ToString(),
                    data = data
                }
            };
            var response = await _jsCommunicator.SendRequestMessage<MintNftRequest<MintEvmNftPayload>, BoolPayloadResponse>(RequestResponseMessageTypes.MintNft, payload, ct);

            return response.result;
        }
        public async UniTask<string> SendRawTransaction(string address, string amount, string chainId, string data, CancellationToken ct = default)
        {
            var request = new SendRawTransactionRequest<EvmPayload>
            {
                type = ChainTypes.Evm,
                destinationAddress = address,
                amount = amount,
                payload = new EvmPayload
                {
                    chainId = chainId,
                    data = data
                }
            };

            var response = await _jsCommunicator.SendRequestMessage<SendRawTransactionRequest<EvmPayload>, SendRawTransactionResponse>(RequestResponseMessageTypes.SendRawTransaction, request, ct);
            if (string.IsNullOrEmpty(response.error))
                throw new ElympicsException($"Couldn't send transaction: {response.error}");
            return response.txHash;
        }
    }
}
