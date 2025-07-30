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
        IReadOnlyDictionary<Guid, CoinInfo> ElympicsCoins { get; }

        /// <summary>
        /// All currently available battle wallets mapped to coins they store using coin IDs.
        /// The availability of specific wallets is determined by the game configuration, current platform and wallets that a user has connected to their account.
        /// </summary>
        [PublicAPI]
        IReadOnlyDictionary<Guid, VirtualDepositInfo> UserDepositCollection { get; }

        [PublicAPI]
        event Action<VirtualDepositInfo>? VirtualDepositUpdated;

        [PublicAPI]
        event Action<CoinInfo>? VirtualDepositRemoved;

        /// <summary>Show a window in PlayPad where the player will be able to add or withdraw funds from their battle wallet.</summary>
        /// <param name="coinId">ID of the coin for which battle wallet should be used in the window.</param>
        [PublicAPI]
        UniTask DisplayDepositPopup(Guid coinId, CancellationToken ct = default);
        /// <summary>Update <see cref="UserDepositCollection"/> with latest data.</summary>
        /// <returns>A reference to <see cref="UserDepositCollection"/>.</returns>
        UniTask<IReadOnlyDictionary<Guid, VirtualDepositInfo>> GetVirtualDeposit(CancellationToken ct = default);
        [Obsolete("This method no longer needs to be called and will be removed in the future. The process of ensuring proper deposit values is now handled internally by the SDK.")]
        UniTask<EnsureDepositInfo> EnsureVirtualDeposit(decimal amount, CoinInfo coinInfo, CancellationToken ct = default) => UniTask.FromResult(new EnsureDepositInfo { Success = true });
        UniTask<IReadOnlyDictionary<Guid, CoinInfo>> GetElympicsCoins(CancellationToken ct = default);

        UniTask<WalletBalanceInfo> GetConnectedWalletCurrencyBalance(Guid coinId, CancellationToken ct = default);

        UniTask<WalletBalanceInfo> GetWalletCurrencyBalance(string walletAddress, Guid coinId, CancellationToken ct = default);
    }
}
