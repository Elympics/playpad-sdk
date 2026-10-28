#nullable enable

using Elympics.Core.Logger;
using Elympics.Cysharp.Threading.Tasks;
using ElympicsPlayPad.Lobby;
using ElympicsPlayPad.Rooms.Models;

namespace ElympicsPlayPad.Rooms.Mappers
{
    internal static class Mappers
    {
        internal static async UniTask<CoinInfo> ToCoinInfo(this RoomCoin coin, LoggerConfig logger)
        {
            var chainInfo = new ChainInfo
            {
                ExternalId = coin.Chain.ExternalId,
                Type = coin.Chain.Type.ToString(),
                Name = coin.Chain.Name
            };

            var currencyInfo = new CurrencyInfo
            {
                Ticker = coin.Currency.Ticker,
                Address = coin.Currency.Address,
                Decimals = coin.Currency.Decimals,
                Icon = await CoinIcons.GetIconOrNull(coin.CoinId, coin.Currency.IconUrl, logger)
            };

            var coinInfo = new CoinInfo
            {
                Id = coin.CoinId,
                Chain = chainInfo,
                Currency = currencyInfo
            };
            return coinInfo;
        }
    }
}
