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
    public static class VirtualDepositExt
    {
        internal static async UniTask<VirtualDepositInfo> ToVirtualDepositInfo(this DepositResponse response, Texture2D? cachedTexture, ElympicsLoggerContext logger)
        {
            var currencyInfo = new CurrencyInfo
            {
                Ticker = response.currency.ticker,
                Address = response.currency.address,
                Decimals = response.currency.decimals,
                Icon = cachedTexture ? cachedTexture : await GetIcon(response.currency.iconUrl, logger)
            };

            var chainInfo = new ChainInfo
            {
                Type = response.currency.chainType,
                Name = response.currency.chainName,
                ExternalId = response.currency.chainExternalId
            };

            var coinInfo = new CoinInfo
            {
                Id = Guid.Parse(response.currency.coinId),
                Currency = currencyInfo,
                Chain = chainInfo
            };

            var depositInfo = new VirtualDepositInfo
            {
                Amount = WeiConverter.FromWei(response.amount, currencyInfo.Decimals),
                Wei = response.amount,
                CoinInfo = coinInfo
            };
            return depositInfo;
        }

        internal static async UniTask<VirtualDepositInfo> ToVirtualDepositInfo(this DepositUpdated response, Texture2D? cachedTexture, ElympicsLoggerContext logger)
        {
            var currencyInfo = new CurrencyInfo
            {
                Ticker = response.currency.ticker,
                Address = response.currency.address,
                Decimals = response.currency.decimals,
                Icon = cachedTexture ? cachedTexture : await GetIcon(response.currency.iconUrl, logger)
            };

            var chainInfo = new ChainInfo
            {
                Type = response.currency.chainType,
                Name = response.currency.chainName,
                ExternalId = response.currency.chainExternalId
            };

            var coinInfo = new CoinInfo
            {
                Id = Guid.Parse(response.currency.coinId),
                Currency = currencyInfo,
                Chain = chainInfo
            };

            var depositInfo = new VirtualDepositInfo
            {
                Amount = WeiConverter.FromWei(response.amount, currencyInfo.Decimals),
                Wei = response.amount,
                CoinInfo = coinInfo
            };
            return depositInfo;
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
