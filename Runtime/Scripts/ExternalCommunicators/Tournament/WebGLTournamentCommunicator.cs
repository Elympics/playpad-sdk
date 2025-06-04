#nullable enable
using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.ElympicsSystems.Internal;
using ElympicsPlayPad.ExternalCommunicators.Tournament.Extensions;
using ElympicsPlayPad.ExternalCommunicators.VirtualDeposit;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Requests;
using ElympicsPlayPad.Protocol.Responses;
using ElympicsPlayPad.Protocol.WebMessages;
using ElympicsPlayPad.Tournament.Data;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Tournament
{
    internal class WebGLTournamentCommunicator : IExternalTournamentCommunicator, IWebMessageReceiver
    {
        public event Action<TournamentInfo>? TournamentUpdated;
        public TournamentInfo? CurrentTournament { get; private set; }

        private readonly IExternalBlockChainCurrencyCommunicator _blockChainCurrencyCommunicator;
        private readonly JsCommunicator _jsCommunicator;
        private readonly ElympicsLoggerContext _logger;

        public WebGLTournamentCommunicator(ElympicsLoggerContext logger, IExternalBlockChainCurrencyCommunicator blockChainCurrencyCommunicator, JsCommunicator jsCommunicator)
        {
            _blockChainCurrencyCommunicator = blockChainCurrencyCommunicator;
            _jsCommunicator = jsCommunicator;
            _logger = logger.WithContext(nameof(WebGLTournamentCommunicator));
            _jsCommunicator.RegisterIWebEventReceiver(this, WebMessageTypes.TournamentUpdated);
        }

        public async UniTask<TournamentInfo?> GetTournament(CancellationToken ct = default)
        {
            var response = await _jsCommunicator.SendRequestMessage<EmptyPayload, TournamentResponse>(RequestResponseMessageTypes.GetTournament, null, ct);
            CurrentTournament = response.ToTournamentInfo();
            return CurrentTournament.Value;
        }
        public async UniTask<TournamentFeeInfo?> GetRollTournamentsFee(TournamentFeeRequestInfo[] requestData, CancellationToken ct = default)
        {
            if (_blockChainCurrencyCommunicator.ElympicsCoins is null)
                throw new NullReferenceException($"Can't request fee when {_blockChainCurrencyCommunicator.ElympicsCoins} is null.");

            if (requestData.Length == 0)
                return null;

            var message = new TournamentFeeRequest
            {
                rollings = new RollingDetail[requestData.Length]
            };

            foreach (var (requestInfo, index) in requestData.Select((value, i) => (value, i)))
                message.rollings[index] = new RollingDetail
                {
                    rollingId = Guid.NewGuid().ToString(),
                    coinId = requestInfo.CoinInfo.Id.ToString(),
                    playersCount = requestInfo.PlayersCount,
                    prize = WeiConverter.ToWei(requestInfo.Prize, requestInfo.CoinInfo.Currency.Decimals)
                };

            var response = await _jsCommunicator.SendRequestMessage<TournamentFeeRequest, TournamentFeeResponse>(RequestResponseMessageTypes.GetRollTournamentFees, message, ct);

            var feesInfo = new FeeInfo[response.rollings.Length];
            foreach (var fee in response.rollings)
            {
                var requestIndex = message.rollings.Select((value, index) => new { value, index }).Where(x => x.value.rollingId == fee.rollingId).Select(x => x.index).First();
                var coinId = requestData[requestIndex].CoinInfo.Id;
                if (!_blockChainCurrencyCommunicator.ElympicsCoins.TryGetValue(coinId, out var coinInfo))
                    throw new ElympicsException("Couldn't find coinInfo.");
                feesInfo[requestIndex] = fee.ToTournamentFeeInfo(coinInfo);
            }
            return new TournamentFeeInfo
            {
                Fees = feesInfo,
            };
        }
        public void OnWebMessage(WebMessage message)
        {
            var logger = _logger.WithMethodName();
            if (!string.Equals(message.type, WebMessageTypes.TournamentUpdated))
                throw logger.CaptureAndThrow(new Exception($"{nameof(WebGLTournamentCommunicator)} can handle only {WebMessageTypes.TournamentUpdated} event type."));
            try
            {
                switch (message.type)
                {
                    case WebMessageTypes.TournamentUpdated:
                        var newTournamentData = JsonUtility.FromJson<TournamentUpdatedMessage>(message.message);
                        CurrentTournament = newTournamentData.ToTournamentInfo();
                        TournamentUpdated?.Invoke(CurrentTournament.Value);
                        break;
                    default:
                        logger.Error($"Unable to handle message {message.type}");
                        break;
                }

            }
            catch (Exception e)
            {
                throw logger.CaptureAndThrow(e);
            }
        }
    }
}
