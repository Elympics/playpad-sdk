#nullable enable
using System;
using System.Numerics;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.Models.Authentication;
using SCS;
using UnityEngine;

namespace ElympicsPlayPad.Web3.Data.Providers
{
    public class DuelInfo : MonoBehaviour
    {
        private Web3Wallet wallet;
        private SmartContractService scs;

        [SerializeField] private TokenInfo tokenInfo;
        [SerializeField] private BalanceInfo balanceInfo;

        public BigInteger? CachedAllowance { get; private set; }
        public event Action InfoUpdated;

        private void Awake()
        {
            wallet = FindObjectOfType<Web3Wallet>();
            scs = FindObjectOfType<SmartContractService>();

            balanceInfo.InfoUpdated += OnInfoUpdated;
        }

        private void OnDestroy()
        {
            balanceInfo.InfoUpdated -= OnInfoUpdated;
        }

        public bool CanFetch
        {
            get
            {
                return ElympicsLobbyClient.Instance.AuthData.AuthType == AuthType.EthAddress
                    && scs.CurrentChain != null
                    && balanceInfo.IsInfoAvailable;
            }
        }

        private async void OnInfoUpdated()
        {
            _ = await Allowance();
        }

        public async UniTask<BigInteger> Allowance()
        {
            if (!CanFetch)
                throw new Exception("Cannot fetch allowance in current state");

            var duelAddress = scs.CurrentChain.Value.GetSmartContract(SmartContractType.OnChainDuel).Address;
            CachedAllowance = await wallet.GetAllowance(duelAddress);
            InfoUpdated?.Invoke();
            return CachedAllowance.Value;
        }

    }
}
