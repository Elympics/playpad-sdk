#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.ElympicsSystems.Internal;
using Elympics.Rooms.Models;
using Elympics.Util;
using ElympicsPlayPad.ExternalCommunicators.VirtualDeposit.Ext;
using ElympicsPlayPad.ExternalCommunicators.VirtualDeposit.Models;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Requests;
using ElympicsPlayPad.Protocol.Responses;
using ElympicsPlayPad.Protocol.WebMessages;
using ElympicsPlayPad.Session;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.VirtualDeposit
{
    internal class WebGLBlockChainCurrencyCommunicator : IExternalBlockChainCurrencyCommunicator, IWebMessageReceiver
    {
        public event Action<VirtualDepositInfo>? VirtualDepositUpdated;
        public event Action<CoinInfo>? VirtualDepositRemoved;
        public IReadOnlyDictionary<Guid, CoinInfo>? ElympicsCoins => _elympicsCoins;
        public IReadOnlyDictionary<Guid, VirtualDepositInfo>? UserDepositCollection => _userDepositCollection;
        private Dictionary<Guid, VirtualDepositInfo>? _userDepositCollection;
        private readonly Dictionary<Guid, VirtualDepositInfo> _tempUpdatedCoinsCache;
        private readonly List<KeyValuePair<Guid, VirtualDepositInfo>> _tempDeletedCoinsCache;
        private readonly JsCommunicator _jsCommunicator;
        private readonly ElympicsLoggerContext _logger;
        private Dictionary<Guid, CoinInfo>? _elympicsCoins;

        public WebGLBlockChainCurrencyCommunicator(JsCommunicator jsCommunicator, ElympicsLoggerContext logger)
        {
            _jsCommunicator = jsCommunicator;
            _tempUpdatedCoinsCache = new Dictionary<Guid, VirtualDepositInfo>();
            _tempDeletedCoinsCache = new List<KeyValuePair<Guid, VirtualDepositInfo>>();
            _logger = logger.WithContext(nameof(WebGLBlockChainCurrencyCommunicator));
            _jsCommunicator.RegisterIWebEventReceiver(this, WebMessageTypes.VirtualDepositUpdated);
        }

        public async UniTask DisplayDepositPopup(Guid coinId, CancellationToken ct = default)
        {
            //TO DO: Add a new message type instead of using ensure with 0 amount
            var request = new EnsureVirtualDepositRequest
            {
                amount = "0",
                coinId = coinId.ToString()
            };
            var response = await _jsCommunicator.SendRequestMessage<EnsureVirtualDepositRequest, EnsureVirtualDepositResponse>(RequestResponseMessageTypes.EnsureVirtualDeposit, request, ct);

            if (!response.success)
                throw new Exception($"Opening deposit popup failed:\n{response.error}");
        }

        public async UniTask<IReadOnlyDictionary<Guid, VirtualDepositInfo>?> GetVirtualDeposit(CancellationToken ct = default)
        {
            var result = await _jsCommunicator.SendRequestMessage<EmptyPayload, VirtualDepositResponse>(RequestResponseMessageTypes.GetVirtualDeposit, null, ct);

            if (result.deposits == null || result.deposits.Length == 0)
                return _userDepositCollection = null;

            _userDepositCollection ??= new Dictionary<Guid, VirtualDepositInfo>();
            _userDepositCollection.Clear();
            foreach (var depositResponse in result.deposits)
            {
                var depositInfo = await depositResponse.ToVirtualDepositInfo(_logger);
                _userDepositCollection.Add(depositInfo.CoinInfo.Id, depositInfo);
            }
            return _userDepositCollection;
        }

        public async UniTask<EnsureDepositInfo> EnsureVirtualDeposit(decimal amount, CoinInfo coinInfo, CancellationToken ct = default)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount of virtual deposit has to be greater than 0", nameof(amount));

            var weiAmount = RawCoinConverter.ToRaw(amount, coinInfo.Currency.Decimals);
            var request = new EnsureVirtualDepositRequest
            {
                amount = weiAmount,
                coinId = coinInfo.Id.ToString()
            };
            var result = await _jsCommunicator.SendRequestMessage<EnsureVirtualDepositRequest, EnsureVirtualDepositResponse>(RequestResponseMessageTypes.EnsureVirtualDeposit, request, ct);
            return new EnsureDepositInfo
            {
                Success = result.success,
                Error = result.error
            };
        }
        public async UniTask<IReadOnlyDictionary<Guid, CoinInfo>?> GetElympicsCoins(CancellationToken ct)
        {
            var result = await _jsCommunicator.SendRequestMessage<EmptyPayload, ElympicsCoinsResponse>(RequestResponseMessageTypes.GetAvailableCoins, null, ct);
            if (result.currencies == null || result.currencies.Length == 0)
                return _elympicsCoins = null;

            _elympicsCoins ??= new Dictionary<Guid, CoinInfo>();
            _elympicsCoins.Clear();
            foreach (var currencyResponse in result.currencies)
            {
                var coinInfo = await currencyResponse.ToCoinInfo(_logger);
                _elympicsCoins[coinInfo.Id] = coinInfo;
            }
            return _elympicsCoins;
        }
        public UniTask<WalletBalanceInfo> GetConnectedWalletCurrencyBalance(Guid coinId, CancellationToken ct = default) => RetrieveBalanceInfo(string.Empty, coinId, ct);

        public UniTask<WalletBalanceInfo> GetWalletCurrencyBalance(string walletAddress, Guid coinId, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(walletAddress))
                throw new ArgumentNullException(nameof(walletAddress));

            return RetrieveBalanceInfo(walletAddress, coinId, ct);
        }

        public async UniTask<SignProofOfEntryResult> SignProofOfEntry(CancellationToken ct = default)
        {
            var currentRoom = ElympicsLobbyClient.Instance?.RoomsManager.CurrentRoom;
            if (currentRoom == null)
                return new SignProofOfEntryResult(false, "Client is not connected to a room.");

            //TO DO: add handling for rolling tournaments
            var betDetails = currentRoom.State.MatchmakingData?.BetDetails;
            if (betDetails == null)
                return new SignProofOfEntryResult(false, "Current room has no bet.");

            var request = new SignProofOfEntryRequest
            {
                amount = betDetails.BetValueRaw,
                coinId = betDetails.Coin.CoinId.ToString(),
                roomId = currentRoom.RoomId.ToString()
            };
            var response = await _jsCommunicator.SendRequestMessage<SignProofOfEntryRequest, ResultPayloadResponse>(RequestResponseMessageTypes.SignProofOfEntry, request, ct);
            return new SignProofOfEntryResult(response.success, response.error);
        }

        public void OnWebMessage(WebMessage message)
        {
            switch (message.type)
            {
                case WebMessageTypes.VirtualDepositUpdated:
                    var webMessage = JsonUtility.FromJson<VirtualDepositUpdatedMessage>(message.message);
                    HandleVirtualDepositUpdatedMessage(webMessage).Forget();
                    break;
                default:
                    break;
            }
        }

        private async UniTask<WalletBalanceInfo> RetrieveBalanceInfo(string walletAddress, Guid coinId, CancellationToken ct)
        {

            if (_elympicsCoins is null || _elympicsCoins.Count == 0)
                throw new ElympicsException($"Can't get available coins. ElympicsCoins is null or empty. Please initialize PlayPad using {nameof(SessionManager.AuthenticateFromExternalAndConnect)}");

            if (!_elympicsCoins.TryGetValue(coinId, out var cachedCoin))
                throw new ElympicsException($"Coin with {coinId} is not recognized.");

            var request = new WalletCurrencyBalanceRequest()
            {
                coinId = coinId.ToString(),
                walletAddress = walletAddress,
            };

            var result = await _jsCommunicator.SendRequestMessage<WalletCurrencyBalanceRequest, WalletCurrencyBalanceResponse>(RequestResponseMessageTypes.GetWalletCurrencyBalance, request, ct);
            return result.ToWalletBalanceInfo(cachedCoin.Currency.Decimals);
        }

        private async UniTaskVoid HandleVirtualDepositUpdatedMessage(VirtualDepositUpdatedMessage message)
        {
            if (message.deposits == null || message.deposits.Length == 0)
            {
                if (_userDepositCollection == null)
                    return;

                var removedDeposits = _userDepositCollection;
                _userDepositCollection = null;

                foreach (var deposit in removedDeposits)
                    VirtualDepositRemoved?.Invoke(deposit.Value.CoinInfo);

                return;
            }
            _tempUpdatedCoinsCache.Clear();
            foreach (var deposit in message.deposits)
            {
                _userDepositCollection ??= new Dictionary<Guid, VirtualDepositInfo>();
                var virtualDepositInfo = await deposit.ToVirtualDepositInfo(_logger);
                _tempUpdatedCoinsCache.Add(virtualDepositInfo.CoinInfo.Id, virtualDepositInfo);
            }

            _tempDeletedCoinsCache.Clear();
            _tempDeletedCoinsCache.AddRange(_userDepositCollection!.Where(kvp => !_tempUpdatedCoinsCache.ContainsKey(kvp.Key)));
            _userDepositCollection!.Clear();
            _userDepositCollection.AddRange(_tempUpdatedCoinsCache);

            foreach (var deletedCoin in _tempDeletedCoinsCache)
                VirtualDepositRemoved?.Invoke(deletedCoin.Value.CoinInfo);
            foreach (var updatedCoin in _userDepositCollection)
                VirtualDepositUpdated?.Invoke(updatedCoin.Value);
        }
    }
}
