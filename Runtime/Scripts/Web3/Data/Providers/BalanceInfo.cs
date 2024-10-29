#nullable enable
using System;
using System.Numerics;
using Elympics;
using Elympics.Models.Authentication;
using UnityEngine;

namespace ElympicsPlayPad.Web3.Data.Providers
{
    public class BalanceInfo : MonoBehaviour
    {
        private Web3Wallet wallet;
        public BigInteger Balance { get; private set; }
        public bool IsInfoAvailable { get; private set; }
        public event Action InfoUpdated;

        private void Awake()
        {
            wallet = FindObjectOfType<Web3Wallet>();
            if (ElympicsLobbyClient.Instance.IsAuthenticated)
                FetchBalance(ElympicsLobbyClient.Instance.AuthData);
            else
                ElympicsLobbyClient.Instance.AuthenticationSucceeded += FetchBalance;
        }

        private async void FetchBalance(AuthData authData)
        {
            if (CanFetch())
            {
                Debug.Log("[BalanceInfo] Fetching Balance");
                Balance = await wallet.GetBalance();
                IsInfoAvailable = true;
                InfoUpdated?.Invoke();
            }
        }

        private bool CanFetch() => ElympicsLobbyClient.Instance.AuthData.AuthType == AuthType.EthAddress;

        public void TriggerReload()
        {
            FetchBalance(ElympicsLobbyClient.Instance.AuthData);
        }

        private void OnDestroy()
        {
            ElympicsLobbyClient.Instance.AuthenticationSucceeded -= FetchBalance;
        }
    }
}
