#nullable enable
using System;
using Elympics;
using Elympics.ElympicsSystems.Internal;
using ElympicsPlayPad.ExternalCommunicators.Authentication;
using ElympicsPlayPad.ExternalCommunicators.GameStatus;
using ElympicsPlayPad.ExternalCommunicators.Leaderboard;
using ElympicsPlayPad.ExternalCommunicators.Sentry;
using ElympicsPlayPad.ExternalCommunicators.Tournament;
using ElympicsPlayPad.ExternalCommunicators.Ui;
using ElympicsPlayPad.ExternalCommunicators.Utility;
using ElympicsPlayPad.ExternalCommunicators.VirtualDeposit;
using ElympicsPlayPad.ExternalCommunicators.Web3.Erc20SmartContract;
using ElympicsPlayPad.ExternalCommunicators.Web3.Wallet;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Session;
using ElympicsPlayPad.Utility;
using ElympicsPlayPad.Wrappers;
using JetBrains.Annotations;
using UnityEngine;
using ElympicsPlayPad.ExternalCommunicators.Web3.ContractOperations;
using ElympicsPlayPad.ExternalCommunicators.Web3.NFT;

namespace ElympicsPlayPad.ExternalCommunicators
{
    [RequireComponent(typeof(JsCommunicator))]
    [RequireComponent(typeof(SessionManager))]
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

        [PublicAPI]
        public IExternalVirtualDepositCommunicator? VirtualDepositCommunicator;

        [PublicAPI]
        public ITonNftExternalCommunicator? TonNftExternalCommunicator;

        [SerializeField] private StandaloneExternalAuthenticatorConfig standaloneAuthConfig = null!;
        [SerializeField] private StandaloneExternalTournamentConfig standaloneTournamentConfig = null!;
        [SerializeField] private StandaloneExternalGameStatusConfig standaloneGameStatusConfig = null!;

        private JsCommunicator _jsCommunicator = null!;
        private WebGLFunctionalities? _webGLFunctionalities;
        private IElympicsLobbyWrapper _lobby = null!;

        private IExternalSentryCommunicator? _sentry;
        internal static ElympicsLoggerContext LoggerContext;

        /// <summary>False in editor and local builds that are not run through PlayPad website.</summary>
        private static bool UseRealPlayPad =>
#if UNITY_WEBGL && !UNITY_EDITOR && !ELYMPICS_DISABLE_PLAYPAD
            true;
#else
            false;
#endif

        private void Awake()
        {
            if (Instance == null)
            {
                var version = PlayPadSdkVersionRetriever.GetVersionStringFromAssembly();
                LoggerContext = ElympicsLogger.CurrentContext ?? new ElympicsLoggerContext(ElympicsLogger.SessionId);
                LoggerContext = LoggerContext.WithApp(ElympicsLoggerContext.PlayPadContextApp).SetPlayPadSdkContext(JsCommunicator.ProtocolVersion, version).WithContext(nameof(PlayPadCommunicator));
                _jsCommunicator = GetComponent<JsCommunicator>();
                if (_jsCommunicator == null)
                    throw new ArgumentNullException(nameof(_jsCommunicator), $"Couldn't find {nameof(JsCommunicator)} component on gameObject {gameObject.name}");
                var gameId = ElympicsConfig.LoadCurrentElympicsGameConfig().GameId;
                _jsCommunicator.Init(LoggerContext);

                _lobby = GetComponent<IElympicsLobbyWrapper>();
                if (_lobby == null)
                    throw new ArgumentNullException(nameof(_jsCommunicator), $"Couldn't find {nameof(IElympicsLobbyWrapper)} component on gameObject {gameObject.name}");

                var sessionmanager = GetComponent<SessionManager>();
                if (sessionmanager == null)
                    throw new ArgumentNullException(nameof(sessionmanager), $"Couldn't find {nameof(SessionManager)} component on gameObject {gameObject.name}");
                sessionmanager.Init(LoggerContext);

                if (UseRealPlayPad)
                {
                    _webGLFunctionalities = new WebGLFunctionalities(_jsCommunicator);
                    ExternalAuthenticator = new WebGLExternalAuthenticator(_jsCommunicator, LoggerContext, sessionmanager);
                    var walletCommunicator = new WebGLExternalWalletCommunicator(_jsCommunicator);
                    TournamentCommunicator = new WebGLTournamentCommunicator(_jsCommunicator);
                    GameStatusCommunicator = new WebGLGameStatusCommunicator(_jsCommunicator, _lobby, TournamentCommunicator!, LoggerContext);
                    ExternalUiCommunicator = new WebGLExternalUiCommunicator(_jsCommunicator);
                    var webGLContractOperations = new WebGLExternalContractOperations(_jsCommunicator);
                    TokenCommunicator = new Erc20SmartContractCommunicator(webGLContractOperations, walletCommunicator);
                    LeaderboardCommunicator = new WebGLLeaderboardCommunicator(_jsCommunicator, LoggerContext);
                    VirtualDepositCommunicator = new WebGLVirtualDepositCommunicator(_jsCommunicator, LoggerContext);
                    _sentry = new WebGLExternalSentryCommunicator(_jsCommunicator);
                    TonNftExternalCommunicator = new WebGLTonNftExternalCommunicator(_jsCommunicator);
                }
                else
                {
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
                    VirtualDepositCommunicator = customVirtualDepositCommunicator != null ? customVirtualDepositCommunicator : null;
                    TonNftExternalCommunicator = customTonNftExternalCommunicator != null ? customTonNftExternalCommunicator : new StandaloneTonNftExternalCommunicator();
                }

                Instance = this;
            }
            else
                Destroy(gameObject);
        }

        [Header("Optional. Works only in Unity Editor.")]
        [SerializeField] private CustomStandaloneAuthenticationCommunicatorBase? customAuthenticatorCommunicator;

        [SerializeField] private CustomStandaloneLeaderboardCommunicatorBase? customLeaderboardCommunicator;

        [SerializeField] private CustomStandaloneTournamentCommunicatorBase? customTournamentCommunicator;

        [SerializeField] private CustomStandaloneGameStatusCommunicatorBase? customGameStatusCommunicator;

        [SerializeField] private CustomStandaloneExternalUiCommunicatorBase? customExternalUiCommunicator;

        [SerializeField] private CustomStandaloneErc20SmartContractCommunicatorBase? customErc20SmartContractCommunicator;

        [SerializeField] private CustomStandaloneVirtualDepositCommunicatorBase? customVirtualDepositCommunicator;

        [SerializeField] private CustomTonNftExternalCommunicator? customTonNftExternalCommunicator;

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
