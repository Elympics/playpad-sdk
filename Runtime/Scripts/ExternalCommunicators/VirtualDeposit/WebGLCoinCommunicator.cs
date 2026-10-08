#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics.Core.Logger;
using ElympicsPlayPad.ExternalCommunicators.VirtualDeposit.Ext;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Lobby;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Responses;

namespace ElympicsPlayPad.ExternalCommunicators.VirtualDeposit
{
    internal class WebGLCoinCommunicator : IExternalCoinCommunicator
    {
        public IReadOnlyDictionary<Guid, CoinInfo> ElympicsCoins => _elympicsCoins;
        private readonly PlayPadMessagingSystem _playPadMessagingSystem;
        private readonly LoggerConfig _logger = ElympicsLogger.WithPlayPadSdkService()
            .WithClass(typeof(WebGLCoinCommunicator))
            .WithMonitoringEnabled();
        private readonly Dictionary<Guid, CoinInfo> _elympicsCoins = new();

        public WebGLCoinCommunicator(PlayPadMessagingSystem playPadMessagingSystem) => _playPadMessagingSystem = playPadMessagingSystem;

        public async UniTask<IReadOnlyDictionary<Guid, CoinInfo>> GetElympicsCoins(CancellationToken ct)
        {
            var result = await _playPadMessagingSystem.SendRequestMessage<EmptyPayload, ElympicsCoinsResponse>(RequestResponseMessageTypes.GetAvailableCoins, null, ct);
            _elympicsCoins.Clear();

            if (result.currencies == null)
                return _elympicsCoins;

            foreach (var currencyResponse in result.currencies)
            {
                var coinInfo = await currencyResponse.ToCoinInfo(_logger);
                _elympicsCoins[coinInfo.Id] = coinInfo;
            }

            return _elympicsCoins;
        }

        async UniTask<CoinInfo> IExternalCoinCommunicator.GetCoinInfo(Guid coinId, CancellationToken ct) => await UniTask.FromResult(CoinInfo.CreateNullCoinInfo());
    }
}
