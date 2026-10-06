#nullable enable

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics.Util;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Lobby;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Requests;
using ElympicsPlayPad.Protocol.Responses;
#if UNITY_EDITOR || !UNITY_WEBGL || ELYMPICS_PLAYPAD_KEEP_LOBBY
using ElympicsPlayPad.Rooms;
#endif

namespace ElympicsPlayPad.ExternalCommunicators.VirtualDeposit
{
    internal static class VirtualDepositOperations
    {
#if UNITY_EDITOR || !UNITY_WEBGL || ELYMPICS_PLAYPAD_KEEP_LOBBY
        public static async UniTask<SignProofOfEntryResult> SignProofOfEntry(PlayPadMessagingSystem jsCommunicator, IRoom room, CancellationToken ct)
        {
            var betDetails = room.State.MatchmakingData?.BetDetails;
            if (betDetails == null)
                return new SignProofOfEntryResult(false, "Current room has no bet.");

            var request = new SignProofOfEntryRequest
            {
                amount = betDetails.BetValueRaw,
                coinId = betDetails.Coin.CoinId.ToString(),
                roomId = room.RoomId.ToString(),
            };
            var response = await jsCommunicator.SendRequestMessage<SignProofOfEntryRequest, ResultPayloadResponse>(RequestResponseMessageTypes.SignProofOfEntry, request, ct);
            return new SignProofOfEntryResult(response.success, response.error);
        }
#endif

        public static async UniTask<EnsureDepositInfo> EnsureVirtualDeposit(PlayPadMessagingSystem jsCommunicator, decimal amount, CoinInfo coinInfo, CancellationToken ct = default)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount of virtual deposit has to be greater than 0", nameof(amount));

            var weiAmount = RawCoinConverter.ToRaw(amount, coinInfo.Currency.Decimals);
            var request = new EnsureVirtualDepositRequest
            {
                amount = weiAmount,
                coinId = coinInfo.Id.ToString(),
            };
            var result = await jsCommunicator.SendRequestMessage<EnsureVirtualDepositRequest, EnsureVirtualDepositResponse>(RequestResponseMessageTypes.EnsureVirtualDeposit, request, ct);
            return new EnsureDepositInfo
            {
                Success = result.success,
                Error = result.error,
            };
        }
    }
}
