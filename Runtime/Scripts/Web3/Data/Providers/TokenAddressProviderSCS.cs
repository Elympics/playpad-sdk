#nullable enable
using System;
using SCS;
using UnityEngine;

namespace ElympicsPlayPad.Web3.Data.Providers
{
    public class TokenAddressProviderSCS : MonoBehaviour, ITokenAddressProvider
    {
        private SmartContractService _scs = null!;

        private void Awake() => _scs = FindObjectOfType<SmartContractService>();

        public string GetAddress()
        {
            if (!_scs.CurrentChain.HasValue)
                throw new Exception("Cannot access current chain data");
            return _scs.CurrentChain.Value.GetSmartContract(SmartContractType.ERC20Token).Address;
        }

        public int GetChainId()
        {
            if (!_scs.CurrentChain.HasValue)
                throw new Exception("Cannot access current chain data");
            return int.Parse(_scs.CurrentChain.Value.ChainId);
        }
    }
}
