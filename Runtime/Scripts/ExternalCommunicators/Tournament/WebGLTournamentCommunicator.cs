#nullable enable
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.ElympicsSystems.Internal;
using Elympics.Util;
using ElympicsPlayPad.ExternalCommunicators.Tournament.Extensions;
using ElympicsPlayPad.ExternalCommunicators.Tournament.Models;
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
        private readonly IJsCommunicator _jsCommunicator;
        private readonly ElympicsLoggerContext _logger;

        public WebGLTournamentCommunicator(ElympicsLoggerContext logger, IExternalBlockChainCurrencyCommunicator blockChainCurrencyCommunicator, IJsCommunicator jsCommunicator)
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
                rollings = new RollingDetail[requestData.Length],
            };

            foreach (var (requestInfo, index) in requestData.Select((value, i) => (value, i)))
                message.rollings[index] = new RollingDetail
                {
                    coinId = requestInfo.CoinInfo.Id.ToString(),
                    playersCount = requestInfo.PlayersCount,
                    prize = RawCoinConverter.ToRaw(requestInfo.Prize, requestInfo.CoinInfo.Currency.Decimals),
                    prizeDistribution = requestInfo.PrizeDistribution?.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray() ?? Array.Empty<string>(),
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
                    take = maxCount,
                },
                ct);

            if (response.entries is null)
                return new RollingTournamentHistory(Array.Empty<RollingTournamentHistoryEntry>());

            var entries = new RollingTournamentHistoryEntry[response.entries.Length];
            for (var i = 0; i < response.entries.Length; i++)
            {
                var entry = response.entries[i];
                entry.scores ??= Array.Empty<RollingTournamentScore>();
                entry.prizes ??= Array.Empty<string>();
                var tournamentCoin = await FetchCoinForHistoryMatch(Guid.Parse(entry.coinId));
                entries[i] = ToPublicModel(entry, tournamentCoin);
            }
            return new RollingTournamentHistory(entries);

            RollingTournamentHistoryEntry ToPublicModel(GetRollingTournamentHistoryResponse.HistoryEntry entry, CoinInfo coinInfo)
            {
                var logger = _logger.WithMethodName();
                var allMatches = entry.scores.OrderBy(participation => participation.position).Select(x => ParticipationToMatch(x, coinInfo)).ToList().AsReadOnly();
                if (!entry.scores.Any(score => score.mine))
                    throw logger.CaptureAndThrow(new ElympicsException("Received list of all matches in a rolling tournament does not contain local player's match."));
                var localPlayerMatch = ParticipationToMatch(entry.scores.First(score => score.mine), coinInfo);
                var localPlayerMatchIndex = allMatches.IndexOf(localPlayerMatch);

                var prizes = entry.prizes.Select(x => RawCoinConverter.FromRaw(x, coinInfo.Currency.Decimals)).ToArray();
                var entryFee = RawCoinConverter.FromRaw(entry.entryFee, coinInfo.Currency.Decimals);
                RollingTournamentPrizeDetails? prizeDetails = new RollingTournamentPrizeDetails(coinInfo, entryFee, prizes);

                var state = entry.state switch
                {
                    nameof(RollingTournamentHistoryEntry.TournamentState.Live) => RollingTournamentHistoryEntry.TournamentState.Live,
                    nameof(RollingTournamentHistoryEntry.TournamentState.Finished) => RollingTournamentHistoryEntry.TournamentState.Finished,
                    nameof(RollingTournamentHistoryEntry.TournamentState.YourResultsPending) => RollingTournamentHistoryEntry.TournamentState.YourResultsPending,
                    nameof(RollingTournamentHistoryEntry.TournamentState.Cancelled) => RollingTournamentHistoryEntry.TournamentState.Cancelled,
                    _ => RollingTournamentHistoryEntry.TournamentState.Unknown,
                };

                if (state == RollingTournamentHistoryEntry.TournamentState.Unknown)
                    logger.Error($"Unexpected rolling tournament state '{entry.state}' received.");

                return new RollingTournamentHistoryEntry(state, prizeDetails, entry.numberOfPlayers, allMatches, localPlayerMatchIndex, entry.unreadSettled);
            }

            RollingTournamentMatch ParticipationToMatch(RollingTournamentScore rollingScore, CoinInfo coinInfo)
            {
                var logger = _logger.WithMethodName();
                if (!DateTime.TryParse(rollingScore.matchEnded, out var matchEnded))
                {
                    matchEnded = DateTime.MinValue;
                    logger.Error($"Received match end date and time is in invalid format: {rollingScore.matchEnded}. SDK will return {matchEnded} instead.");
                }

                var matchState = ConvertToMatchState(rollingScore.state, logger);
                return new RollingTournamentMatch(rollingScore.avatar,
                    rollingScore.nickname,
                    matchEnded,
                    rollingScore.score,
                    matchState,
                    RawCoinConverter.FromRaw(rollingScore.prize, coinInfo.Currency.Decimals),
                    rollingScore.position);
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


        public async UniTask<TournamentInfo> SetActiveTournament(string tournamentId, CancellationToken ct = default)
        {
            var payload = new SetActiveTournamentRequest { tournamentId = tournamentId };
            var response = await _jsCommunicator.SendRequestMessage<SetActiveTournamentRequest, TournamentUpdatedMessage>(RequestResponseMessageTypes.SetActiveTournament, payload, ct);

            if (string.IsNullOrEmpty(response.id))
                throw new ArgumentException($"Tournament with ID {tournamentId} does not exist.", nameof(tournamentId));

            CurrentTournament = response.ToTournamentInfo();
            return CurrentTournament.Value;
        }

        public async UniTask<RollingTournamentDetails> GetRollingTournamentDetails(Guid matchId, CancellationToken ct = default)
        {
            var payload = new GetRollingTournamentDetailsRequest { matchId = matchId.ToString() };
            var response = await _jsCommunicator.SendRequestMessage<GetRollingTournamentDetailsRequest, GetRollingTournamentDetailsResponse>(RequestResponseMessageTypes.GetRollingTournamentDetails,
                payload,
                ct);

            var tournamentState = response.state switch
            {
                nameof(RollingTournamentDetails.TournamentState.Live) => RollingTournamentDetails.TournamentState.Live,
                nameof(RollingTournamentDetails.TournamentState.Finished) => RollingTournamentDetails.TournamentState.Finished,
                nameof(RollingTournamentDetails.TournamentState.YourResultsPending) => RollingTournamentDetails.TournamentState.YourResultsPending,
                nameof(RollingTournamentDetails.TournamentState.Cancelled) => RollingTournamentDetails.TournamentState.Cancelled,
                _ => RollingTournamentDetails.TournamentState.Unknown,
            };

            if (tournamentState == RollingTournamentDetails.TournamentState.Unknown)
                _logger.Error($"Unexpected rolling tournament state '{response.state}' received.");

            var coinInfo = await FetchCoinForHistoryMatch(Guid.Parse(response.coinId));

            var prizes = response.prizes.Select(x => RawCoinConverter.FromRaw(x, coinInfo.Currency.Decimals)).ToArray();
            var entryFee = RawCoinConverter.FromRaw(response.entryFee, coinInfo.Currency.Decimals);
            RollingTournamentPrizeDetails? prizeDetails = new RollingTournamentPrizeDetails(coinInfo, entryFee, prizes);

            var matches = new RollingTournamentMatchDetails[response.scores.Length];

            //Order by position, but treat 0 as no position (failed or unfinished match)
            var orderedResponseMatches = response.scores.OrderBy(x => x.position > 0 ? x.position : uint.MaxValue).ToList();

            var localPlayerMatchIndex = -1;
            for (var i = 0; i < matches.Length; i++)
            {
                var match = orderedResponseMatches[i];
                var matchState = ConvertToMatchState(match.state, _logger);
                DateTime? matchEnded = string.IsNullOrEmpty(match.matchEnded) ? null : DateTime.Parse(match.matchEnded);
                uint? position = match.position > 0 ? match.position : null;

                matches[i] = new RollingTournamentMatchDetails(matchState, match.avatar, match.nickname, matchEnded, match.score, position);

                if (match.mine)
                {
                    if (localPlayerMatchIndex > -1)
                        _logger.WithMethodName().Error($"Received multiple matches from a rolling tournament with {nameof(RollingTournamentScore.mine)} set to true.");

                    localPlayerMatchIndex = i;
                }
            }

            return new RollingTournamentDetails(tournamentState, prizeDetails, response.numberOfPlayers, Array.AsReadOnly(matches), localPlayerMatchIndex);
        }

        private static MatchState ConvertToMatchState(string matchState, ElympicsLoggerContext logger)
        {
            switch (matchState)
            {
                case "Failed":
                    return MatchState.Failed;
                case "Finished":
                    return MatchState.Finished;
                case "Playing":
                    return MatchState.Playing;
                default:
                    logger.Error($"Unexpected match state '{matchState}' received.");
                    return MatchState.Unknown;
            }
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

        private async UniTask<CoinInfo> FetchCoinForHistoryMatch(Guid coinId)
        {
            if (_blockChainCurrencyCommunicator.ElympicsCoins.TryGetValue(coinId, out var coin))
                return coin;

            return await _blockChainCurrencyCommunicator.GetCoinInfo(coinId, CancellationToken.None);
        }
    }
}
