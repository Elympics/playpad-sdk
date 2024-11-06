#nullable enable
using ElympicsPlayPad.Utility;
using SCS;
using UnityEngine;

namespace ElympicsPlayPad.Wrappers
{
    [RequireComponent(typeof(SmartContractService))]
    [DefaultExecutionOrder(ElympicsLobbyExecutionOrders.DefaultSmartContractServiceWrapper)]
    public class DefaultSmartContractServiceWrapper : MonoBehaviour, ISmartContractServiceWrapper
    {
        private SmartContractService _scs;
        private void Awake()
        {
            _scs = GetComponent<SmartContractService>();
        }
        public ChainConfig? CurrentChain => _scs.CurrentChain;
        public void RegisterWallet(IWallet wallet) => _scs.RegisterWallet(wallet);
    }
}
