using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.ExternalCommunicators.VirtualDeposit.Models;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.VirtualDeposit
{
    public abstract class CustomStandaloneVirtualDepositCommunicatorBase : MonoBehaviour, IExternalVirtualDepositCommunicator
    {
        public abstract IReadOnlyDictionary<Guid, CoinInfo> ElympicsCoins { get; }
        public abstract IReadOnlyDictionary<Guid, VirtualDepositInfo> UserDepositCollection { get; }
        public abstract event Action<VirtualDepositInfo> VirtualDepositUpdated;
        public event Action<CoinInfo> VirtualDepositRemoved;

        public abstract UniTask DisplayDepositPopup(Guid coinId, CancellationToken ct = default);
        public abstract UniTask<IReadOnlyDictionary<Guid, VirtualDepositInfo>> GetVirtualDeposit(CancellationToken ct = default);
        public abstract UniTask<EnsureDepositInfo> EnsureVirtualDeposit(decimal amount, CoinInfo coinInfo, CancellationToken ct = default);
        public abstract UniTask<IReadOnlyDictionary<Guid, CoinInfo>> GetElympicsCoins(CancellationToken ct);
    }
}
