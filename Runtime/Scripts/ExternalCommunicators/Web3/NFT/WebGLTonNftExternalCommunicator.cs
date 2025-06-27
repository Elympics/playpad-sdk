#nullable enable

using System.Threading;
using Cysharp.Threading.Tasks;
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
                    type = NftChainTypes.Ton,
                    payload = new MintTonNftPayload { payload = payload }
                },
                ct);

            return response.result;
        }
    }
}
