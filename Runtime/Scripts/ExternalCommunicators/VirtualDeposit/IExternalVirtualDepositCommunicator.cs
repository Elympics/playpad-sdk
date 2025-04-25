#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.ExternalCommunicators.VirtualDeposit.Models;
using JetBrains.Annotations;

namespace ElympicsPlayPad.ExternalCommunicators.VirtualDeposit
{
    public interface IExternalVirtualDepositCommunicator
    {
        [PublicAPI]
        IReadOnlyDictionary<Guid, VirtualDepositInfo>? UserDepositCollection { get; }

        [PublicAPI]
        public event Action<VirtualDepositInfo>? VirtualDepositUpdated;

        [PublicAPI]
        UniTask DisplayDepositPopup(Guid coinId, CancellationToken ct = default);

        UniTask<IReadOnlyDictionary<Guid, VirtualDepositInfo>?> GetVirtualDeposit(CancellationToken ct = default);
        UniTask<EnsureDepositInfo> EnsureVirtualDeposit(decimal amount, CoinInfo coinInfo, CancellationToken ct = default);
    }
}
