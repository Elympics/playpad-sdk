#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Elympics.ElympicsSystems.Internal;
using ElympicsPlayPad.ExternalCommunicators.VirtualDeposit.Models;
using ElympicsPlayPad.Protocol.Responses;
using ElympicsPlayPad.Protocol.WebMessages;
using UnityEngine;
using UnityEngine.Networking;

namespace ElympicsPlayPad.ExternalCommunicators.VirtualDeposit.Ext
{
    internal static class BlockChainCoinsExt
    {
        public static async UniTask<VirtualDepositInfo> ToVirtualDepositInfo(this DepositResponse response, Texture2D? cachedTexture, ElympicsLoggerContext logger)
        {

            var coinInfo = await response.currency.ToCoinInfo(cachedTexture, logger);

            var depositInfo = new VirtualDepositInfo
            {
                Amount = WeiConverter.FromWei(response.amount, coinInfo.Currency.Decimals),
                Wei = response.amount,
                CoinInfo = coinInfo
            };
            return depositInfo;
        }

        public static async UniTask<CoinInfo> ToCoinInfo(this CurrencyResponse currencyResponse, Texture2D? cachedTexture, ElympicsLoggerContext logger)
        {
            var currencyInfo = new CurrencyInfo
            {
                Ticker = currencyResponse.ticker,
                Address = currencyResponse.address,
                Decimals = currencyResponse.decimals,
                Icon = cachedTexture ? cachedTexture : await GetIcon(currencyResponse.iconUrl, logger)
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

        public static async UniTask<VirtualDepositInfo> ToVirtualDepositInfo(this DepositUpdated response, Texture2D? cachedTexture, ElympicsLoggerContext logger)
        {
            var coinInfo = await response.currency.ToCoinInfo(cachedTexture, logger);

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

        private static async UniTask<CoinInfo> ToCoinInfo(this CurrencyUpdated currencyResponse, Texture2D? cachedTexture, ElympicsLoggerContext logger)
        {
            var currencyInfo = new CurrencyInfo
            {
                Ticker = currencyResponse.ticker,
                Address = currencyResponse.address,
                Decimals = currencyResponse.decimals,
                Icon = cachedTexture ? cachedTexture : await GetIcon(currencyResponse.iconUrl, logger)
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

        private static async UniTask<Texture2D?> GetIcon(string url, ElympicsLoggerContext logger)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            var log = logger.WithMethodName();
            try
            {
                using var request = await UnityWebRequestTexture.GetTexture(url).SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                    return DownloadHandlerTexture.GetContent(request);

                log.Error($"Failed to download token Icon from {url}. Reason: {request.error}");
                return null;
            }
            catch (Exception e)
            {
                log.Exception(e);
                return null;
            }
        }
    }
}
