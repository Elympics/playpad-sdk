#nullable enable
using System;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.DTO;
using ElympicsPlayPad.Web3.Data;
using ElympicsPlayPad.Wrappers;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.Trust
{
    public class StandardExternalTrustSmartContractOperations : IExternalTrustSmartContractOperations
    {
        private readonly ISmartContractServiceWrapper? _scs;
        public StandardExternalTrustSmartContractOperations(ISmartContractServiceWrapper? scs) => _scs = scs;
        public void ShowTrustPanel() => Debug.Log("Mock Deposit.");
        public async UniTask<TrustState> GetTrustState()
        {
            if (_scs == null)
                throw new NullReferenceException($"Make sure that {nameof(PlayPadCommunicator)} gameObject has {nameof(ISmartContractServiceWrapper)} component attached.");

            return await _scs.GetTrustBalance();
        }
    }
}
