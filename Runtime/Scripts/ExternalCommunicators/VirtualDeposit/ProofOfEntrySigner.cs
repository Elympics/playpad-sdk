#nullable enable

using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Requests;
using ElympicsPlayPad.Protocol.Responses;

namespace ElympicsPlayPad.ExternalCommunicators.VirtualDeposit
{
    internal class ProofOfEntrySigner
    {
        private readonly JsCommunicator _jsCommunicator;

        public ProofOfEntrySigner(JsCommunicator jsCommunicator) => _jsCommunicator = jsCommunicator;

        public async UniTask<SignProofOfEntryResult> SignProofOfEntry(IRoom room, CancellationToken ct)
        {
            var betDetails = room.State.MatchmakingData?.BetDetails;
            if (betDetails == null)
                return new SignProofOfEntryResult(false, "Current room has no bet.");

            var request = new SignProofOfEntryRequest
            {
                amount = betDetails.BetValueRaw,
                coinId = betDetails.Coin.CoinId.ToString(),
                roomId = room.RoomId.ToString()
            };
            var response = await _jsCommunicator.SendRequestMessage<SignProofOfEntryRequest, ResultPayloadResponse>(RequestResponseMessageTypes.SignProofOfEntry, request, ct);
            return new SignProofOfEntryResult(response.success, response.error);
        }
    }
}
