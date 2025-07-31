using System.Numerics;
using System.Threading;
using Cysharp.Threading.Tasks;
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
                type = NftChainTypes.Evm,
                payload = new MintEvmNftPayload
                {
                    chainId = chainId.ToString(),
                    data = data
                }
            };
            var response = await _jsCommunicator.SendRequestMessage<MintNftRequest<MintEvmNftPayload>, BoolPayloadResponse>(RequestResponseMessageTypes.MintNft, payload, ct);

            return response.result;
        }
    }
}
