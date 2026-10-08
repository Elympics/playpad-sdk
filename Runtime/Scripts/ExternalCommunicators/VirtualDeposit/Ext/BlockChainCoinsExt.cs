#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Elympics.Core.Logger;
using ElympicsPlayPad.Lobby;
using ElympicsPlayPad.Protocol.Responses;

namespace ElympicsPlayPad.ExternalCommunicators.VirtualDeposit.Ext
{
    internal static class BlockChainCoinsExt
    {
        public static async UniTask<CoinInfo> ToCoinInfo(this CurrencyResponse currencyResponse, LoggerConfig logger)
        {
            var currencyInfo = new CurrencyInfo
            {
                Ticker = currencyResponse.ticker,
                Address = currencyResponse.address,
                Decimals = currencyResponse.decimals,
                Icon = await CoinIcons.GetIconOrNull(Guid.Parse(currencyResponse.coinId), currencyResponse.iconUrl, logger),
            };

            var chainInfo = new ChainInfo
            {
                Type = currencyResponse.chainType,
                Name = currencyResponse.chainName,
                ExternalId = currencyResponse.chainExternalId,
            };

            var coinInfo = new CoinInfo
            {
                Id = Guid.Parse(currencyResponse.coinId),
                Currency = currencyInfo,
                Chain = chainInfo,
            };
            return coinInfo;
        }
    }
}
