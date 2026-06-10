#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.Communication.Mappers;
using Elympics.Core.Logger;
using ElympicsPlayPad.ExternalCommunicators.Authentication;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;
using ElympicsPlayPad.ExternalCommunicators.GameStatus;
using ElympicsPlayPad.ExternalCommunicators.Internal;
using ElympicsPlayPad.ExternalCommunicators.Leaderboard;
using ElympicsPlayPad.ExternalCommunicators.Lobby;
using ElympicsPlayPad.ExternalCommunicators.Replay;
using ElympicsPlayPad.ExternalCommunicators.Sentry;
using ElympicsPlayPad.ExternalCommunicators.Tournament;
using ElympicsPlayPad.ExternalCommunicators.Ui;
using ElympicsPlayPad.ExternalCommunicators.Utility;
using ElympicsPlayPad.ExternalCommunicators.VirtualDeposit;
using ElympicsPlayPad.ExternalCommunicators.Web;
using ElympicsPlayPad.ExternalCommunicators.Web3.ContractOperations;
using ElympicsPlayPad.ExternalCommunicators.Web3.Erc20SmartContract;
using ElympicsPlayPad.ExternalCommunicators.Web3.NFT;
using ElympicsPlayPad.ExternalCommunicators.Web3.Wallet;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Session;
using ElympicsPlayPad.Utility;
using ElympicsPlayPad.Wrappers;
using JetBrains.Annotations;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators
{
    [RequireComponent(typeof(JsCommunicator))]
    [RequireComponent(typeof(SessionManager))]
    [DefaultExecutionOrder(ElympicsLobbyExecutionOrders.ExternalCommunicator)]
    public class PlayPadCommunicator : MonoBehaviour
    {
        /// <summary>Singleton instance of this component.</summary>
        /// <remarks>This field is set in the <see cref="Awake"/> method, so before that method executes value of this field is null.</remarks>
        [PublicAPI] public static PlayPadCommunicator? Instance;

        /// <summary>Gives direct access to methods and events related to authentication and regions.</summary>
        /// <remarks>Most of the time it's better to use <see cref="SessionManager"/> instead of directly accessing this field.</remarks>
        [PublicAPI] public IExternalAuthenticator? ExternalAuthenticator;

        /// <summary>Allows for easy fetching of playability status and starting matches in tournaments.</summary>
        [PublicAPI] public IExternalGameStatusCommunicator? GameStatusCommunicator;

        /// <summary>Contains methods for opening PlayPad modal windows over the game.</summary>
        [PublicAPI] public IExternalUiCommunicator? ExternalUiCommunicator;

        [PublicAPI]
        public IExternalERC20SmartContractOperations? TokenCommunicator;

        [PublicAPI]
        public IExternalTournamentCommunicator? TournamentCommunicator;

        [PublicAPI]
        public IExternalLeaderboardCommunicator? LeaderboardCommunicator;

        [PublicAPI]
        public IExternalReplayCommunicator? ReplayCommunicator;

        [PublicAPI]
        public IExternalBlockChainCurrencyCommunicator? VirtualDepositCommunicator;

        [PublicAPI]
        public ITonNftExternalCommunicator? TonNftExternalCommunicator;

        [PublicAPI]
        public IEvmExternalCommunicator? EvmExternalCommunicator;

        [PublicAPI]
        public IExternalWebCommunicator? ExternalWebCommunicator;

        [PublicAPI]
        public IExternalLobbyCommunicator? LobbyCommunicator;

        [NonSerialized]
        [PublicAPI]
        public ISessionManager SessionManager = null!;

        private PlayPadCommunicatorInternal _communicatorInternal = null!;
        private PlayPadMessagingSystem _playPadMessagingSystem = null!;
        private WebGLFunctionalities? _webGLFunctionalities;
        private IElympicsLobbyWrapper _lobby = null!;

        private IExternalSentryCommunicator? _sentry;
        private IHeartbeatCommunicator? _heartbeat;
        private readonly LoggerConfig _loggerContext = ElympicsLogger.WithPlayPadSdkService()
            .WithClass(typeof(PlayPadCommunicator));

        /// <summary>False in editor and local builds that are not run through PlayPad website.</summary>
        private static bool CanMockPlayPad =>
#if UNITY_EDITOR || ELYMPICS_DISABLE_PLAYPAD
            true;
#else
            false;
#endif

        private void Awake()
        {
            if (!Instance)
            {
                if (transform.parent != null)
                    transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
                ElympicsLogger.State.SetPlayPadVersion(PlayPadMessagingSystem.ProtocolVersion);
                _playPadMessagingSystem = GetComponent<PlayPadMessagingSystem>();
                if (!_playPadMessagingSystem)
                    throw new ArgumentNullException(nameof(_playPadMessagingSystem), $"Couldn't find {nameof(PlayPadMessagingSystem)} component on gameObject {gameObject.name}");
                var playpadCommunicationFactory = GetComponent<PlayPadCommunicatorFactory>();
                if (!playpadCommunicationFactory)
                    throw new ArgumentNullException($"Couldn't find {nameof(PlayPadCommunicatorFactory)} component on gameObject {gameObject.name}");
                _playPadMessagingSystem.Init(playpadCommunicationFactory);

                _lobby = GetComponent<IElympicsLobbyWrapper>();
                if (_lobby is null)
                    throw new ArgumentNullException(nameof(_playPadMessagingSystem), $"Couldn't find {nameof(IElympicsLobbyWrapper)} component on gameObject {gameObject.name}");

                var gameConfig = ElympicsConfig.LoadCurrentElympicsGameConfig();
                if (!gameConfig)
                    throw new ArgumentNullException(nameof(gameConfig),
                        "Elympics Game Config is missing. Please make sure you have an ElympicsGameConfig asset in your project and it is properly configured.");

                var sessionManager = GetComponent<SessionManager>();
                if (sessionManager == null)
                    throw new ArgumentNullException(nameof(sessionManager), $"Couldn't find {nameof(sessionManager)} component on gameObject {gameObject.name}");

                ReplayCommunicator = new WebGLExternalReplay(_playPadMessagingSystem, _lobby);
                _communicatorInternal = new PlayPadCommunicatorInternal(ReplayCommunicator, gameConfig!);

                var authFactory = new AuthFactory();
                authFactory.RegisterAuthProvider(GetComponent<IElympicsLobbyWrapper>(), LaunchMode.Lobby | LaunchMode.Gameplay);
                authFactory.RegisterAuthProvider(_communicatorInternal, LaunchMode.Gameplay);
                sessionManager.Init(authFactory, _playPadMessagingSystem);
                SessionManager = sessionManager;

                _webGLFunctionalities = new WebGLFunctionalities(_playPadMessagingSystem);
                _heartbeat = new WebGLHeartbeatCommunicator(_playPadMessagingSystem);
                ExternalAuthenticator = UseMockAuth
                    ? mockConfiguration!.customAuthenticatorCommunicator
                    : new WebGLExternalAuthenticator(_playPadMessagingSystem, sessionManager, _heartbeat);
                var walletCommunicator = new WebGLExternalWalletCommunicator(_playPadMessagingSystem);
                VirtualDepositCommunicator = UseMockBlockChainCurrency
                    ? mockConfiguration!.customBlockChainCurrencyCommunicator
                    : new WebGLBlockChainCurrencyCommunicator(_playPadMessagingSystem);
                TournamentCommunicator = UseMockTournament
                    ? mockConfiguration!.customTournamentCommunicator
                    : new WebGLTournamentCommunicator(VirtualDepositCommunicator!, _playPadMessagingSystem);
                GameStatusCommunicator = UseMockGameStatus
                    ? mockConfiguration!.customGameStatusCommunicator
                    : new WebGLGameStatusCommunicator(_playPadMessagingSystem, _lobby, TournamentCommunicator!);
                ExternalUiCommunicator = UseMockExternalUi
                    ? mockConfiguration!.customExternalUiCommunicator
                    : new WebGLExternalUiCommunicator(_playPadMessagingSystem);
                var webGLContractOperations = new WebGLExternalContractOperations(_playPadMessagingSystem);
                TokenCommunicator = UseMockErc20SmartContract
                    ? mockConfiguration!.customErc20SmartContractCommunicator
                    : new Erc20SmartContractCommunicator(webGLContractOperations, walletCommunicator);
                LeaderboardCommunicator = UseMockLeaderboard
                    ? mockConfiguration!.customLeaderboardCommunicator
                    : new WebGLLeaderboardCommunicator(_playPadMessagingSystem);
                TonNftExternalCommunicator = UseMockTonNft
                    ? mockConfiguration!.customTonNftExternalCommunicator
                    : new WebGLTonNftExternalCommunicator(_playPadMessagingSystem);
                EvmExternalCommunicator = UseMockEvm
                    ? mockConfiguration!.customEvmExternalCommunicator
                    : new WebGLEvmExternalCommunicator(_playPadMessagingSystem);
                LobbyCommunicator = UseMockLobby ? mockConfiguration!.customLobbyExternalCommunicator : new WebGLExternalLobby(_playPadMessagingSystem);
                Room.BeforeMarkYourselfReady = BeforeSetReady;
                ExternalWebCommunicator = new WebGLWebCommunicator(_playPadMessagingSystem);
                _sentry = new WebGLExternalSentryCommunicator(_playPadMessagingSystem);
                LobbyRegister.PlayPadLobby = _communicatorInternal;
                Instance = this;
            }
            else
                Destroy(gameObject);
        }

        private async UniTask BeforeSetReady(IRoom room, CancellationToken ct)
        {
            if (room.State.MatchmakingData?.BetDetails is not { } betDetails)
                return;

            var logger = _loggerContext.WithMethodName();

            var coinInfo = await betDetails.Coin.ToCoinInfo(logger);
            var ensureVirtualDepositResult = await VirtualDepositOperations.EnsureVirtualDeposit(_playPadMessagingSystem, betDetails.BetValue, coinInfo, ct);
            if (!ensureVirtualDepositResult.Success)
                throw logger.LogExceptionAndReturn(new ElympicsException(ensureVirtualDepositResult.Error));

            var signProofOfEntryResult = await VirtualDepositOperations.SignProofOfEntry(_playPadMessagingSystem, room, ct);
            if (!signProofOfEntryResult.IsSuccess)
                throw logger.LogExceptionAndReturn(new ElympicsException(signProofOfEntryResult.Error));
        }

        [Header("Custom implementations of communicators (works only in Editor)")]
        [SerializeField] private bool useMockConfiguration;

        [SerializeField] private PlayPadMockConfiguration? mockConfiguration;


        private void OnDestroy()
        {
            if (_playPadMessagingSystem)
                _playPadMessagingSystem.Deinit();
            _webGLFunctionalities?.Dispose();
            GameStatusCommunicator?.Dispose();
            _heartbeat?.Dispose();
        }

        private bool UseMockAuth => useMockConfiguration
            && mockConfiguration
            && mockConfiguration!.useStandaloneAuthenticationCommunicator
            && mockConfiguration!.customAuthenticatorCommunicator
            && CanMockPlayPad;

        private bool UseMockBlockChainCurrency => useMockConfiguration
            && mockConfiguration
            && mockConfiguration!.useCustomStandaloneBlockChainCurrencyCommunicator
            && mockConfiguration!.customBlockChainCurrencyCommunicator
            && CanMockPlayPad;

        private bool UseMockTournament => useMockConfiguration
            && mockConfiguration
            && mockConfiguration!.useCustomStandaloneTournamentCommunicator
            && mockConfiguration!.customTournamentCommunicator
            && CanMockPlayPad
            && VirtualDepositCommunicator != null;

        private bool UseMockGameStatus => useMockConfiguration
            && mockConfiguration
            && mockConfiguration!.useCustomStandaloneGameStatusCommunicator
            && mockConfiguration!.customGameStatusCommunicator
            && CanMockPlayPad;

        private bool UseMockExternalUi => useMockConfiguration
            && mockConfiguration
            && mockConfiguration!.useCustomStandaloneExternalUiCommunicator
            && mockConfiguration!.customExternalUiCommunicator
            && CanMockPlayPad;

        private bool UseMockErc20SmartContract => useMockConfiguration
            && mockConfiguration
            && mockConfiguration!.useCustomStandaloneErc20SmartContractCommunicator
            && mockConfiguration!.customErc20SmartContractCommunicator
            && CanMockPlayPad;

        private bool UseMockLeaderboard => useMockConfiguration
            && mockConfiguration
            && mockConfiguration!.useCustomStandaloneLeaderboardCommunicator
            && mockConfiguration!.customLeaderboardCommunicator
            && CanMockPlayPad;

        private bool UseMockTonNft => useMockConfiguration
            && mockConfiguration
            && mockConfiguration!.useCustomTonNftExternalCommunicator
            && mockConfiguration!.customTonNftExternalCommunicator
            && CanMockPlayPad;

        private bool UseMockEvm => useMockConfiguration
            && mockConfiguration
            && mockConfiguration!.useCustomEvmExternalCommunicator
            && mockConfiguration!.customEvmExternalCommunicator
            && CanMockPlayPad;

        private bool UseMockLobby => useMockConfiguration
            && mockConfiguration
            && mockConfiguration!.useCustomLobbyCommunicator
            && mockConfiguration!.customLobbyExternalCommunicator
            && CanMockPlayPad;

        #region internal

        internal const string ExternalAuthenticatorFieldName = nameof(ExternalAuthenticator);
        internal const string TournamentCommunicatorFieldName = nameof(TournamentCommunicator);
        internal const string GameStatusCommunicatorFieldName = nameof(GameStatusCommunicator);
        internal const string VirtualDepositCommunicatorFieldName = nameof(VirtualDepositCommunicator);

        #endregion
    }
}
