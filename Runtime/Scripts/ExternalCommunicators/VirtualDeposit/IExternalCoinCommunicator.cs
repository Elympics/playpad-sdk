#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.Lobby;
using JetBrains.Annotations;

namespace ElympicsPlayPad.ExternalCommunicators.VirtualDeposit
{
    public interface IExternalCoinCommunicator
    {
        [PublicAPI]
        IReadOnlyDictionary<Guid, CoinInfo> ElympicsCoins { get; }

        UniTask<IReadOnlyDictionary<Guid, CoinInfo>> GetElympicsCoins(CancellationToken ct = default);

        internal UniTask<CoinInfo> GetCoinInfo(Guid coinId, CancellationToken ct = default);
    }
}
