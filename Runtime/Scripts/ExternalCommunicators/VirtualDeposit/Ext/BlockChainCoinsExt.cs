#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.Communication.Mappers;
using Elympics.ElympicsSystems.Internal;
using ElympicsPlayPad.ExternalCommunicators.VirtualDeposit.Models;
using ElympicsPlayPad.Protocol.Responses;
using ElympicsPlayPad.Protocol.WebMessages;

namespace ElympicsPlayPad.ExternalCommunicators.VirtualDeposit.Ext
{
    internal static class BlockChainCoinsExt
    {
        public static async UniTask<VirtualDepositInfo> ToVirtualDepositInfo(this DepositResponse response, ElympicsLoggerContext logger)
        {

            var coinInfo = await response.currency.ToCoinInfo(logger);

            var depositInfo = new VirtualDepositInfo
            {
                Amount = WeiConverter.FromWei(response.amount, coinInfo.Currency.Decimals),
                Wei = response.amount,
                CoinInfo = coinInfo
            };
            return depositInfo;
        }

        public static async UniTask<CoinInfo> ToCoinInfo(this CurrencyResponse currencyResponse, ElympicsLoggerContext logger)
        {
            var currencyInfo = new CurrencyInfo
            {
                Ticker = currencyResponse.ticker,
                Address = currencyResponse.address,
                Decimals = currencyResponse.decimals,
                Icon = await CoinIcons.GetIconOrNull(Guid.Parse(currencyResponse.coinId), currencyResponse.iconUrl, logger)
            };

            var chainInfo = new ChainInfo
            {
                Type = currencyResponse.chainType,
                Name = currencyResponse.chainName,
                ExternalId = currencyResponse.chainExternalId
            };

            var coinInfo = new CoinInfo
            {
                Id = Guid.Parse(currencyResponse.coinId),
                Currency = currencyInfo,
                Chain = chainInfo
            };
            return coinInfo;
        }

        public static async UniTask<VirtualDepositInfo> ToVirtualDepositInfo(this DepositUpdated response, ElympicsLoggerContext logger)
        {
            var coinInfo = await response.currency.ToCoinInfo(logger);

            var depositInfo = new VirtualDepositInfo
            {
                Amount = WeiConverter.FromWei(response.amount, coinInfo.Currency.Decimals),
                Wei = response.amount,
                CoinInfo = coinInfo
            };
            return depositInfo;
        }

        public static WalletBalanceInfo ToWalletBalanceInfo(this WalletCurrencyBalanceResponse response, int decimals)
        {
            return new WalletBalanceInfo
            {
                AmountRaw = string.IsNullOrEmpty(response.error) ? response.amount : string.Empty,
                Amount = string.IsNullOrEmpty(response.error) ? WeiConverter.FromWei(response.amount, decimals) : 0,
                Error = response.error
            };
        }

        private static async UniTask<CoinInfo> ToCoinInfo(this CurrencyUpdated currencyResponse, ElympicsLoggerContext logger)
        {
            var currencyInfo = new CurrencyInfo
            {
                Ticker = currencyResponse.ticker,
                Address = currencyResponse.address,
                Decimals = currencyResponse.decimals,
                Icon = await CoinIcons.GetIconOrNull(Guid.Parse(currencyResponse.coinId), currencyResponse.iconUrl, logger)
            };

            var chainInfo = new ChainInfo
            {
                Type = currencyResponse.chainType,
                Name = currencyResponse.chainName,
                ExternalId = currencyResponse.chainExternalId
            };

            var coinInfo = new CoinInfo
            {
                Id = Guid.Parse(currencyResponse.coinId),
                Currency = currencyInfo,
                Chain = chainInfo
            };
            return coinInfo;
        }
    }
}
