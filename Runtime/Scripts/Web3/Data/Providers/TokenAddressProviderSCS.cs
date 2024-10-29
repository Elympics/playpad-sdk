#nullable enable
using System;
using SCS;
using UnityEngine;

namespace ElympicsPlayPad.Web3.Data.Providers
{
    public class TokenAddressProviderSCS : MonoBehaviour, ITokenAddressProvider
    {
        private SmartContractService scs;

        private void Awake()
        {
            scs = FindObjectOfType<SmartContractService>();
        }

        public string GetAddress()
        {
            if (!scs.CurrentChain.HasValue)
                throw new Exception("Cannot access current chain data");
            return scs.CurrentChain.Value.GetSmartContract(SmartContractType.ERC20Token).Address;
        }

        public int GetChainId()
        {
            if (!scs.CurrentChain.HasValue)
                throw new Exception("Cannot access current chain data");
            return int.Parse(scs.CurrentChain.Value.ChainId);
        }
    }
}
