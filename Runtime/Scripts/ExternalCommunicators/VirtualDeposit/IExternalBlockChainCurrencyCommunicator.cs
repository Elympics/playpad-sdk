#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using ElympicsPlayPad.ExternalCommunicators.VirtualDeposit.Models;
using JetBrains.Annotations;

namespace ElympicsPlayPad.ExternalCommunicators.VirtualDeposit
{
    public interface IExternalBlockChainCurrencyCommunicator
    {
        [PublicAPI]
        IReadOnlyDictionary<Guid, CoinInfo>? ElympicsCoins { get; }

        [PublicAPI]
        IReadOnlyDictionary<Guid, VirtualDepositInfo>? UserDepositCollection { get; }

        [PublicAPI]
        event Action<VirtualDepositInfo>? VirtualDepositUpdated;

        [PublicAPI]
        event Action<CoinInfo>? VirtualDepositRemoved;

        [PublicAPI]
        UniTask DisplayDepositPopup(Guid coinId, CancellationToken ct = default);
        UniTask<IReadOnlyDictionary<Guid, VirtualDepositInfo>?> GetVirtualDeposit(CancellationToken ct = default);
        UniTask<EnsureDepositInfo> EnsureVirtualDeposit(decimal amount, CoinInfo coinInfo, CancellationToken ct = default);
        UniTask<IReadOnlyDictionary<Guid, CoinInfo>?> GetElympicsCoins(CancellationToken ct = default);

        UniTask<WalletBalanceInfo> GetConnectedWalletCurrencyBalance(Guid coinId, CancellationToken ct = default);

        UniTask<WalletBalanceInfo> GetWalletCurrencyBalance(string walletAddress, Guid coinId, CancellationToken ct = default);
    }
}
