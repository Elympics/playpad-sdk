#nullable enable

using System.Threading;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Requests;
using ElympicsPlayPad.Protocol.Responses;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.NFT
{
    internal class PlayPadTonNftExternalCommunicator : ITonNftExternalCommunicator
    {
        private const string Type = "TON";
        private readonly JsCommunicator _jsCommunicator;

        internal PlayPadTonNftExternalCommunicator(JsCommunicator jsCommunicator) => _jsCommunicator = jsCommunicator;

        public async UniTask<bool> MintNft(string collectionAddress, string payload, CancellationToken ct = default)
        {
            var response = await _jsCommunicator.SendRequestMessage<MintNftRequest<MintTonNftPayload>, BoolPayloadResponse>(
                ReturnEventTypes.MintNft,
                new MintNftRequest<MintTonNftPayload>
                {
                    collectionAddress = collectionAddress,
                    type = Type,
                    payload = new MintTonNftPayload { payload = payload }
                },
                ct);

            return response.result;
        }
    }
}
