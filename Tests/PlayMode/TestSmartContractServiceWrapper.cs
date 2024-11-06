using ElympicsPlayPad.Wrappers;
using SCS;
using UnityEngine;

namespace ElympicsPlayPad.Tests.PlayMode
{
    public class TestSmartContractServiceWrapper : MonoBehaviour, ISmartContractServiceWrapper
    {
        public ChainConfig? CurrentChain => new ChainConfig()
        {
            ChainId = "11155111",
        };

        public void RegisterWallet(IWallet wallet) => Debug.Log("Wallet registered.");

    }
}
