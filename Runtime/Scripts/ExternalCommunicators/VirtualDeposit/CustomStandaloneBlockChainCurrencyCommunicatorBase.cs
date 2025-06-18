using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using ElympicsPlayPad.ExternalCommunicators.VirtualDeposit.Models;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.VirtualDeposit
{
    public abstract class CustomStandaloneBlockChainCurrencyCommunicatorBase : MonoBehaviour, IExternalBlockChainCurrencyCommunicator
    {
        public abstract IReadOnlyDictionary<Guid, CoinInfo> ElympicsCoins { get; }
        public abstract IReadOnlyDictionary<Guid, VirtualDepositInfo> UserDepositCollection { get; }
        public abstract event Action<VirtualDepositInfo> VirtualDepositUpdated;
        public event Action<CoinInfo> VirtualDepositRemoved;

        public abstract UniTask DisplayDepositPopup(Guid coinId, CancellationToken ct = default);
        public abstract UniTask<IReadOnlyDictionary<Guid, VirtualDepositInfo>> GetVirtualDeposit(CancellationToken ct = default);
        public abstract UniTask<EnsureDepositInfo> EnsureVirtualDeposit(decimal amount, CoinInfo coinInfo, CancellationToken ct = default);
        public abstract UniTask<IReadOnlyDictionary<Guid, CoinInfo>> GetElympicsCoins(CancellationToken ct);
        public abstract UniTask<WalletBalanceInfo> GetConnectedWalletCurrencyBalance(Guid coinId, CancellationToken ct = default);
        public abstract UniTask<WalletBalanceInfo> GetWalletCurrencyBalance(string walletAddress, Guid coinId, CancellationToken ct = default);
    }
}
