#nullable enable
using System;
using Elympics.Models.Authentication;
using ElympicsPlayPad.DTO;
using ElympicsPlayPad.ExternalCommunicators.Authentication;
using ElympicsPlayPad.ExternalCommunicators.GameStatus;
using ElympicsPlayPad.ExternalCommunicators.Leaderboard;
using ElympicsPlayPad.ExternalCommunicators.Tournament;
using ElympicsPlayPad.ExternalCommunicators.Utility;
using ElympicsPlayPad.ExternalCommunicators.Web3.Erc20SmartContract;
using ElympicsPlayPad.ExternalCommunicators.Web3.Trust;
using ElympicsPlayPad.ExternalCommunicators.Web3.Wallet;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Utility;
using ElympicsPlayPad.Web3.Data;
using ElympicsPlayPad.Wrappers;
using JetBrains.Annotations;
using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using ElympicsPlayPad.ExternalCommunicators.Web3.ContractOperations;
#endif

namespace ElympicsPlayPad.ExternalCommunicators
{
    [RequireComponent(typeof(JsCommunicator))]
    [DefaultExecutionOrder(ElympicsLobbyExecutionOrders.ExternalCommunicator)]
    public class PlayPadCommunicator : MonoBehaviour, IPlayPadEventListener
    {
        [PublicAPI]
        public event Action<string, string>? WalletConnected;

        [PublicAPI]
        public event Action? WalletDisconnected;

        [PublicAPI]
        public event Action<TrustDepositInfo>? TrustDepositCompleted;

        [PublicAPI]
        public event Action<AuthData>? AuthDataChanged;

        [PublicAPI]
        public static PlayPadCommunicator? Instance;

        [PublicAPI]
        public IExternalAuthenticator? ExternalAuthenticator;

        [PublicAPI]
        public IExternalWalletCommunicator? WalletCommunicator;

        [PublicAPI]
        public IExternalGameStatusCommunicator? GameStatusCommunicator;

        [PublicAPI]
        public IExternalERC20SmartContractOperations? TokenCommunicator;

        [PublicAPI]
        public IExternalTrustSmartContractOperations? TrustCommunicator;

        [PublicAPI]
        public IExternalTournamentCommunicator? TournamentCommunicator;

        [PublicAPI]
        public IExternalLeaderboardCommunicator? LeaderboardCommunicator;

        [SerializeField] private StandaloneExternalAuthenticatorConfig standaloneAuthConfig = null!;
        [SerializeField] private StandaloneExternalTournamentConfig standaloneTournamentConfig = null!;
        [SerializeField] private StandaloneWalletConfig standaloneWalletConfig = null!;

        private JsCommunicator _jsCommunicator = null!;
        private WebGLFunctionalities? _webGLFunctionalities;
        private IElympicsLobbyWrapper _lobby;
        private ISmartContractServiceWrapper? _scsWrapper;

        private void Awake()
        {
            if (Instance == null)
            {
                _jsCommunicator = GetComponent<JsCommunicator>();
                if (_jsCommunicator == null)
                    throw new ArgumentNullException(nameof(_jsCommunicator), $"Couldn't find {nameof(JsCommunicator)} component on gameObject {gameObject.name}");

                _lobby = GetComponent<IElympicsLobbyWrapper>();
                if (_lobby == null)
                    throw new ArgumentNullException(nameof(_jsCommunicator), $"Couldn't find {nameof(IElympicsLobbyWrapper)} component on gameObject {gameObject.name}");


                _scsWrapper = GetComponent<ISmartContractServiceWrapper>();

#if UNITY_WEBGL && !UNITY_EDITOR
                _webGLFunctionalities = new WebGLFunctionalities(_jsCommunicator);
                ExternalAuthenticator = new WebGLExternalAuthenticator(_jsCommunicator);
                WalletCommunicator = new WebGLExternalWalletCommunicator(_jsCommunicator, _scsWrapper);
                GameStatusCommunicator = new WebGLGameStatusCommunicator(_jsCommunicator, _lobby);
                var webGLContractOperations = new WebGLExternalContractOperations(_jsCommunicator);
                TokenCommunicator = new Erc20SmartContractCommunicator(webGLContractOperations, WalletCommunicator);
                TrustCommunicator = new WebGlTrustSmartContractCommunicator(_jsCommunicator, _lobby);
                TournamentCommunicator = new WebGLTournamentCommunicator(_jsCommunicator);
                LeaderboardCommunicator = new WebGLLeaderboardCommunicator(_jsCommunicator);

#else
                var standaloneCommunicator = new StandaloneWalletCommunicator(standaloneWalletConfig);
                ExternalAuthenticator = new StandaloneExternalAuthenticator(standaloneAuthConfig, standaloneTournamentConfig);
                WalletCommunicator = standaloneCommunicator;
                TokenCommunicator = new Erc20SmartContractCommunicator(standaloneCommunicator, standaloneCommunicator);
                TrustCommunicator = new StandardExternalTrustSmartContractOperations(_scsWrapper);
                GameStatusCommunicator = new StandaloneExternalGameStatusCommunicator();
                TournamentCommunicator = customTournamentCommunicator != null ? customTournamentCommunicator : new StandaloneTournamentCommunicator(standaloneTournamentConfig, _jsCommunicator);
                LeaderboardCommunicator = customLeaderboardCommunicator != null ? customLeaderboardCommunicator : new StandaloneLeaderboardCommunicator();
#endif
                Instance = this;
                WalletCommunicator.SetPlayPadEventListener(this);
                ExternalAuthenticator.SetPlayPadEventListener(this);
            }
            else
                Destroy(gameObject);
        }

#if UNITY_EDITOR || !UNITY_WEBGL
        [Header("Optional. Work only on UnityEditor")]
        [SerializeField] private CustomStandaloneLeaderboardCommunicatorBase? customLeaderboardCommunicator;

        [SerializeField] private CustomStandaloneTournamentCommunicatorBase? customTournamentCommunicator;


        [PublicAPI]
        public void SetCustomExternalAuthenticator(IExternalAuthenticator customExternalAuthenticator)
        {
            ExternalAuthenticator = customExternalAuthenticator ?? throw new ArgumentNullException(nameof(customExternalAuthenticator));
            ExternalAuthenticator.SetPlayPadEventListener(this);
        }

        [PublicAPI]
        public void SetCustomExternalWalletCommunicator(IExternalWalletCommunicator customExternalWalletCommunicator) => WalletCommunicator = customExternalWalletCommunicator ?? throw new ArgumentNullException(nameof(customExternalWalletCommunicator));
        [PublicAPI]
        public void SetCustomExternalGameStatusCommunicator(IExternalGameStatusCommunicator customExternalGameStatusCommunicator) => GameStatusCommunicator = customExternalGameStatusCommunicator ?? throw new ArgumentNullException(nameof(customExternalGameStatusCommunicator));

        [PublicAPI]
        public void SetCustomERC20TokenCommunicator(IExternalERC20SmartContractOperations customExternalErc20SmartContractOperations) => TokenCommunicator = customExternalErc20SmartContractOperations ?? throw new ArgumentNullException(nameof(customExternalErc20SmartContractOperations));
        [PublicAPI]
        public void SetCustomTrustTokenCommunicator(IExternalTrustSmartContractOperations customExternalTrustSmartContractOperations) => TrustCommunicator = customExternalTrustSmartContractOperations ?? throw new ArgumentNullException(nameof(customExternalTrustSmartContractOperations));
#endif

        private void OnDestroy()
        {
            WalletCommunicator?.Dispose();
            _webGLFunctionalities?.Dispose();
            GameStatusCommunicator?.Dispose();
        }
        public void OnWalletConnected(string address, string chainId) => WalletConnected?.Invoke(address, chainId);
        public void OnWalletDisconnected() => WalletDisconnected?.Invoke();
        public void OnAuthChanged(AuthData authData) => AuthDataChanged?.Invoke(authData);
        void IPlayPadEventListener.OnTrustDepositFinished(TrustDepositTransactionFinishedWebMessage transaction)
        {
            var currentTrustState = new TrustState()
            {
                AvailableAmount = transaction.trustState.Available,
                TotalAmount = transaction.trustState.totalAmount,
            };
            if (transaction.status == 0)
            {
                var added = transaction.increasedAmount;
                TrustDepositCompleted?.Invoke(new TrustDepositInfo
                {
                    Added = added,
                    TrustState = currentTrustState
                });
            }
            else
            {
                TrustDepositCompleted?.Invoke(new TrustDepositInfo()
                {
                    Added = 0,
                    TrustState = currentTrustState
                });
            }
        }
    }
}
