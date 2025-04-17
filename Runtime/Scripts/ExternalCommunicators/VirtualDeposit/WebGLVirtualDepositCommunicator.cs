#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.ElympicsSystems.Internal;
using ElympicsPlayPad.ExternalCommunicators.VirtualDeposit.Ext;
using ElympicsPlayPad.ExternalCommunicators.VirtualDeposit.Models;
using ElympicsPlayPad.ExternalCommunicators.VirtualDeposit.Utils;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Requests;
using ElympicsPlayPad.Protocol.Responses;
using ElympicsPlayPad.Protocol.WebMessages;
using ElympicsPlayPad.Utility;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.VirtualDeposit
{
    internal class WebGLVirtualDepositCommunicator : IExternalVirtualDepositCommunicator, IWebMessageReceiver
    {
        public event Action<VirtualDepositInfo>? VirtualDepositUpdated;
        public IReadOnlyDictionary<Guid, VirtualDepositInfo>? UserDepositCollection => _userDepositCollection;
        private Dictionary<Guid, VirtualDepositInfo>? _userDepositCollection;
        private readonly JsCommunicator _jsCommunicator;
        private readonly ElympicsLoggerContext _logger;

        public WebGLVirtualDepositCommunicator(JsCommunicator jsCommunicator, ElympicsLoggerContext logger)
        {
            _jsCommunicator = jsCommunicator;
            _logger = logger.WithContext(nameof(WebGLVirtualDepositCommunicator));
            _jsCommunicator.RegisterIWebEventReceiver(this, WebMessageTypes.VirtualDepositUpdated);
        }

        public async UniTask<IReadOnlyDictionary<Guid, VirtualDepositInfo>?> GetVirtualDeposit(CancellationToken ct = default)
        {
            var result = await _jsCommunicator.SendRequestMessage<EmptyPayload, VirtualDepositResponse>(ReturnEventTypes.GetVirtualDeposit, null, ct);

            if (result.deposits == null)
                return _userDepositCollection;

            _userDepositCollection ??= new Dictionary<Guid, VirtualDepositInfo>();
            _userDepositCollection.Clear();
            foreach (var depositResponse in result.deposits)
            {
                var iconFound = CachedCoinIcons.CoinIcons.TryGetValue(depositResponse.currency.coinId, out var icon);
                var depositInfo = await depositResponse.ToVirtualDepositInfo(icon, _logger);
                if (iconFound is false)
                    CachedCoinIcons.CoinIcons.Add(depositResponse.currency.coinId, depositInfo.CoinInfo.Currency.Icon);
                _userDepositCollection.Add(depositInfo.CoinInfo.Id, depositInfo);
            }
            return _userDepositCollection;
        }

        public async UniTask<EnsureDepositInfo> EnsureVirtualDeposit(decimal amount, CoinInfo coinInfo, CancellationToken ct = default)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount of virtual deposit has to be greater than 0", nameof(amount));

            var weiAmount = WeiConverter.ToWei(amount, coinInfo.Currency.Decimals);
            var request = new EnsureVirtualDepositRequest
            {
                amount = weiAmount,
                coinId = coinInfo.Id.ToString()
            };
            var result = await _jsCommunicator.SendRequestMessage<EnsureVirtualDepositRequest, EnsureVirtualDepositResponse>(ReturnEventTypes.EnsureVirtualDeposit, request, ct);
            return new EnsureDepositInfo
            {
                Success = result.success,
                Error = result.error
            };
        }
        public void OnWebMessage(WebMessage message)
        {
            switch (message.type)
            {
                case WebMessageTypes.VirtualDepositUpdated:
                    var webMessage = JsonUtility.FromJson<VirtualDepositUpdatedMessage>(message.message);
                    HandleVirtualDepositUpdatedMessage(webMessage).Forget();
                    break;
            }
        }

        private async UniTaskVoid HandleVirtualDepositUpdatedMessage(VirtualDepositUpdatedMessage message)
        {
            _userDepositCollection ??= new Dictionary<Guid, VirtualDepositInfo>();
            foreach (var deposit in message.deposits)
            {
                var iconFound = CachedCoinIcons.CoinIcons.TryGetValue(deposit.currency.coinId, out var icon);
                var virtualDepositInfo = await deposit.ToVirtualDepositInfo(icon, _logger);
                if (iconFound == false)
                    CachedCoinIcons.CoinIcons.Add(deposit.currency.coinId, virtualDepositInfo.CoinInfo.Currency.Icon);
                _userDepositCollection[virtualDepositInfo.CoinInfo.Id] = virtualDepositInfo;
                VirtualDepositUpdated?.Invoke(virtualDepositInfo);
            }
        }
    }
}
