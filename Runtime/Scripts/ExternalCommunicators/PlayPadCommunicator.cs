#nullable enable
using System;
using ElympicsPlayPad.ExternalCommunicators.Authentication;
using ElympicsPlayPad.ExternalCommunicators.GameStatus;
using ElympicsPlayPad.ExternalCommunicators.Leaderboard;
using ElympicsPlayPad.ExternalCommunicators.Tournament;
using ElympicsPlayPad.ExternalCommunicators.Ui;
using ElympicsPlayPad.ExternalCommunicators.Utility;
using ElympicsPlayPad.ExternalCommunicators.Web3.Erc20SmartContract;
using ElympicsPlayPad.ExternalCommunicators.Web3.Wallet;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Utility;
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
    public class PlayPadCommunicator : MonoBehaviour
    {
        [PublicAPI]
        public static PlayPadCommunicator? Instance;

        [PublicAPI]
        public IExternalAuthenticator? ExternalAuthenticator;

        [PublicAPI]
        public IExternalGameStatusCommunicator? GameStatusCommunicator;

        [PublicAPI]
        public IExternalUiCommunicator? ExternalUiCommunicator;

        [PublicAPI]
        public IExternalERC20SmartContractOperations? TokenCommunicator;

        [PublicAPI]
        public IExternalTournamentCommunicator? TournamentCommunicator;

        [PublicAPI]
        public IExternalLeaderboardCommunicator? LeaderboardCommunicator;

        [SerializeField] private StandaloneExternalAuthenticatorConfig standaloneAuthConfig = null!;
        [SerializeField] private StandaloneExternalTournamentConfig standaloneTournamentConfig = null!;
        [SerializeField] private StandaloneExternalGameStatusConfig standaloneGameStatusConfig = null!;

        private JsCommunicator _jsCommunicator = null!;
        private WebGLFunctionalities? _webGLFunctionalities;
        private IElympicsLobbyWrapper _lobby = null!;

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

#if UNITY_WEBGL && !UNITY_EDITOR && !ELYMPICS_DISABLE_PLAYPAD
                _webGLFunctionalities = new WebGLFunctionalities(_jsCommunicator);
                ExternalAuthenticator = new WebGLExternalAuthenticator(_jsCommunicator);
                var walletCommunicator = new WebGLExternalWalletCommunicator(_jsCommunicator);
                TournamentCommunicator = new WebGLTournamentCommunicator(_jsCommunicator);
                GameStatusCommunicator = new WebGLGameStatusCommunicator(_jsCommunicator, _lobby, TournamentCommunicator!);
                ExternalUiCommunicator = new WebGLExternalUiCommunicator(_jsCommunicator);
                var webGLContractOperations = new WebGLExternalContractOperations(_jsCommunicator);
                TokenCommunicator = new Erc20SmartContractCommunicator(webGLContractOperations, walletCommunicator);
                LeaderboardCommunicator = new WebGLLeaderboardCommunicator(_jsCommunicator);

#else
                if (customErc20SmartContractCommunicator != null)
                    TokenCommunicator = customErc20SmartContractCommunicator;
                else
                {
                    var standaloneCommunicator = new StandaloneWalletCommunicator();
                    ExternalAuthenticator = customAuthenticatorCommunicator != null ? customAuthenticatorCommunicator : new StandaloneExternalAuthenticator(standaloneAuthConfig);
                    TokenCommunicator = new Erc20SmartContractCommunicator(standaloneCommunicator, standaloneCommunicator);
                }
                GameStatusCommunicator = customGameStatusCommunicator != null ? customGameStatusCommunicator : new StandaloneExternalGameStatusCommunicator(standaloneGameStatusConfig, _lobby.RoomsManager);
                ExternalUiCommunicator = customExternalUiCommunicator != null ? customExternalUiCommunicator : new StandaloneExternalUiCommunicator();
                TournamentCommunicator = customTournamentCommunicator != null ? customTournamentCommunicator : new StandaloneTournamentCommunicator(standaloneTournamentConfig, standaloneAuthConfig, _jsCommunicator);
                LeaderboardCommunicator = customLeaderboardCommunicator != null ? customLeaderboardCommunicator : new StandaloneLeaderboardCommunicator();
#endif
                Instance = this;
            }
            else
                Destroy(gameObject);
        }

#if UNITY_EDITOR || !UNITY_WEBGL || ELYMPICS_DISABLE_PLAYPAD
        [Header("Optional. Work only on UnityEditor")]
        [SerializeField] private CustomStandaloneAuthenticationCommunicatorBase? customAuthenticatorCommunicator;

        [SerializeField] private CustomStandaloneLeaderboardCommunicatorBase? customLeaderboardCommunicator;

        [SerializeField] private CustomStandaloneTournamentCommunicatorBase? customTournamentCommunicator;

        [SerializeField] private CustomStandaloneGameStatusCommunicatorBase? customGameStatusCommunicator;

        [SerializeField] private CustomStandaloneExternalUiCommunicatorBase? customExternalUiCommunicator;

        [SerializeField] private CustomStandaloneErc20SmartContractCommunicatorBase? customErc20SmartContractCommunicator;
#endif

        private void OnDestroy()
        {
            _webGLFunctionalities?.Dispose();
            GameStatusCommunicator?.Dispose();
        }

        #region internal

        internal const string ExternalAuthenticatorFieldName = nameof(ExternalAuthenticator);
        internal const string TournamentCommunicatorFieldName = nameof(TournamentCommunicator);
        internal const string GameStatusCommunicatorFieldName = nameof(GameStatusCommunicator);

        #endregion
    }
}
