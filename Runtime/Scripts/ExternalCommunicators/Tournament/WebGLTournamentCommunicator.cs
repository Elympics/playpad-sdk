#nullable enable
using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.ElympicsSystems.Internal;
using Elympics.Util;
using ElympicsPlayPad.ExternalCommunicators.Tournament.Extensions;
using ElympicsPlayPad.ExternalCommunicators.Tournament.Models.MyNamespace;
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
        public async UniTask<TournamentFeeInfo?> GetRollingTournamentsFee(TournamentFeeRequestInfo[] requestData, CancellationToken ct = default)
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
                    coinId = requestInfo.CoinInfo.Id.ToString(),
                    playersCount = requestInfo.PlayersCount,
                    prize = RawCoinConverter.ToRaw(requestInfo.Prize, requestInfo.CoinInfo.Currency.Decimals)
                };

            var response = await _jsCommunicator.SendRequestMessage<TournamentFeeRequest, TournamentFeeResponse>(RequestResponseMessageTypes.GetRollTournamentFees, message, ct);

            var feesInfo = new FeeInfo[response.rollings.Length];
            for (var i = 0; i < response.rollings.Length; i++)
            {
                var coinId = requestData[i].CoinInfo.Id;
                if (!_blockChainCurrencyCommunicator.ElympicsCoins.TryGetValue(coinId, out var coinInfo))
                    throw new ElympicsException("Couldn't find coinInfo.");
                feesInfo[i] = response.rollings[i].ToTournamentFeeInfo(coinInfo);
            }

            return new TournamentFeeInfo
            {
                Fees = feesInfo,
            };
        }

        public async UniTask<RollingTournamentHistory> GetRollingTournamentHistory(uint maxCount, uint skip = 0, CancellationToken ct = default)
        {
            if (_blockChainCurrencyCommunicator.ElympicsCoins is null)
                throw new NullReferenceException($"Can't request rolling tournament history when {_blockChainCurrencyCommunicator.ElympicsCoins} is null.");
            if (maxCount <= 0)
                return new RollingTournamentHistory(Array.Empty<RollingTournamentHistoryEntry>());

            var response = await _jsCommunicator.SendRequestMessage<GetRollingTournamentHistoryRequest, GetRollingTournamentHistoryResponse>(
                RequestResponseMessageTypes.GetRollingTournamentHistory,
                new GetRollingTournamentHistoryRequest
                {
                    skip = skip,
                    take = maxCount
                },
                ct);

            return new RollingTournamentHistory(response.entries.Select(ToPublicModel).ToArray());

            RollingTournamentHistoryEntry ToPublicModel(GetRollingTournamentHistoryResponse.HistoryEntry entry)
            {
                var allMatches = entry.allScores.OrderBy(participation => participation.position).Select(ParticipationToMatch).ToList().AsReadOnly();
                var localPlayerMatch = ParticipationToMatch(entry.myScore);
                var localPlayerMatchIndex = allMatches.IndexOf(localPlayerMatch);

                if (localPlayerMatchIndex < 0)
                    throw _logger.CaptureAndThrow(new ElympicsException($"Received list of all matches in a rolling tournament does not contain local player's match."));

                CoinInfo? coinInfo = null;
                decimal? prize = null;
                decimal? entryFee = null;

                // ReSharper disable once InvertIf
                if (_blockChainCurrencyCommunicator.ElympicsCoins.TryGetValue(Guid.Parse(entry.tournament.coinId), out var coin))
                {
                    coinInfo = coin;
                    prize = RawCoinConverter.FromRaw(entry.tournament.prize, coinInfo.Value.Currency.Decimals);
                    entryFee = RawCoinConverter.FromRaw(entry.tournament.entryFee, coinInfo.Value.Currency.Decimals);
                }
                return new RollingTournamentHistoryEntry(entry.state, prize, coinInfo, entryFee, entry.tournament.numberOfPlayers, allMatches, localPlayerMatchIndex, entry.unreadSettled);
            }

            RollingTournamentMatch ParticipationToMatch(GetRollingTournamentHistoryResponse.Participation participation)
            {
                if (!DateTime.TryParse(participation.matchEnded, out var matchEnded))
                {
                    matchEnded = DateTime.MinValue;
                    _logger.Error($"Received match end date and time is in invalid format: {participation.matchEnded}. SDK will return {matchEnded} instead.");
                }

                return new RollingTournamentMatch(participation.avatar, participation.nickname, matchEnded, participation.score);
            }
        }
        public async UniTask<RollingTournamentSettlementStatus> GetTournamentSettlementStatus(CancellationToken ct = default)
        {
            var result = await _jsCommunicator.SendRequestMessage<EmptyPayload, GetRollingTournamentUnreadSettlementsResponse>(RequestResponseMessageTypes.GetUnreadSettlements, null, ct);
            return new RollingTournamentSettlementStatus
            {
                NewSettlements = result.unreadSettledCount,
            };
        }


        public async UniTask<(bool isSuccess, string? error)> SetActiveTournament(string tournamentId, CancellationToken ct = default)
        {
            var response = await _jsCommunicator.SendRequestMessage<SetActiveTournamentRequest, ResultPayloadResponse>(RequestResponseMessageTypes.SetActiveTournament, new SetActiveTournamentRequest { tournamentId = tournamentId }, ct);
            return (response.success, response.error);
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
