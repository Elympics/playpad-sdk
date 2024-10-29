#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Elympics;
using Elympics.Models.Authentication;
using SCS;
using UnityEngine;

namespace ElympicsPlayPad.Web3.Data.Providers
{
    public class DepositInfo : MonoBehaviour
    {
        private SmartContractService scs;
        private DepositState _state;

        public BigInteger Available
        {
            get
            {
                return _state.AvailableAmount;
            }
        }
        public BigInteger Locked
        {
            get
            {
                return Total - Available;
            }
        }
        public BigInteger Total
        {
            get
            {
                return _state.ActualAmount;
            }
        }

        public bool IsInfoAvailable
        {
            get
            {
                return _state != null;
            }
        }

        private bool CanFetch
        {
            get
            {
                return ElympicsLobbyClient.Instance.AuthData.AuthType == AuthType.EthAddress
                    && scs.CurrentChain != null;

            }
        }

        public event Action InfoUpdated;

        private void Awake()
        {
            scs = FindObjectOfType<SmartContractService>();

            if (ElympicsLobbyClient.Instance.IsAuthenticated)
                UpdateDeposit();
            ElympicsLobbyClient.Instance.AuthenticationSucceeded += OnAuthenticated;
        }

        private void OnDestroy()
        {
            ElympicsLobbyClient.Instance.AuthenticationSucceeded -= OnAuthenticated;
        }

        private void OnAuthenticated(AuthData authData)
        {
            UpdateDeposit();
        }

        private async void UpdateDeposit()
        {
            if (!CanFetch)
                return;

            var task = scs.GetDepositState(ElympicsConfig.LoadCurrentElympicsGameConfig().GameId);
            var expectedTokenAddress = scs.CurrentChain.Value.GetSmartContract(SmartContractType.ERC20Token).Address;

            IReadOnlyList<DepositState> depositStates = await task;
            var deposit = depositStates.First(element => element.TokenAddress.ToLower() == expectedTokenAddress.ToLower());
            if (deposit != null)
            {
                _state = deposit;
                InfoUpdated?.Invoke();
            }
        }

        public void TriggerReload()
        {
            UpdateDeposit();
        }

    }
}
