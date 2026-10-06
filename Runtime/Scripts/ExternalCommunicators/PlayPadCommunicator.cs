#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
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
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Session;
using ElympicsPlayPad.Utility;
using ElympicsPlayPad.Wrappers;
using JetBrains.Annotations;
using UnityEngine;
#if UNITY_EDITOR || !UNITY_WEBGL || ELYMPICS_PLAYPAD_KEEP_LOBBY
using ElympicsPlayPad.Rooms;
using ElympicsPlayPad.Rooms.Mappers;
#endif

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
        public IExternalTournamentCommunicator? TournamentCommunicator;

        [PublicAPI]
        public IExternalLeaderboardCommunicator? LeaderboardCommunicator;

        [PublicAPI]
        public IExternalReplayCommunicator? ReplayCommunicator;

        [PublicAPI]
        public IExternalBlockChainCurrencyCommunicator? VirtualDepositCommunicator;

        [PublicAPI]
        public IExternalWebCommunicator? ExternalWebCommunicator;

        [PublicAPI]
        public IExternalLobbyCommunicator? LobbyCommunicator;

        [NonSerialized]
        [PublicAPI]
        public ISessionManager SessionManager = null!;

#if UNITY_EDITOR || !UNITY_WEBGL || ELYMPICS_PLAYPAD_KEEP_LOBBY
        /// <summary>Elympics lobby: authentication, the lobby connection and Rooms. WebGL builds have it only with ELYMPICS_PLAYPAD_KEEP_LOBBY.</summary>
        [PublicAPI]
        public IElympicsLobbyWrapper Lobby { get; private set; } = null!;
#endif

        private PlayPadCommunicatorInternal _communicatorInternal = null!;
        private PlayPadMessagingSystem _playPadMessagingSystem = null!;
        private WebGLFunctionalities? _webGLFunctionalities;
        private WebGLExternalSentryCommunicator? _sentry;
        private IHeartbeatCommunicator? _heartbeat;
        private readonly LoggerConfig _loggerContext = ElympicsLogger.WithPlayPadSdkService()
            .WithClass(typeof(PlayPadCommunicator))
            .WithMonitoringEnabled();

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

#if UNITY_EDITOR || !UNITY_WEBGL || ELYMPICS_PLAYPAD_KEEP_LOBBY
                Lobby = GetComponent<IElympicsLobbyWrapper>() ?? throw new ArgumentNullException(nameof(_playPadMessagingSystem), $"Couldn't find {nameof(IElympicsLobbyWrapper)} component on gameObject {gameObject.name}");
#endif

                var gameConfig = ElympicsConfig.LoadCurrentElympicsGameConfig();
                if (!gameConfig)
                    throw new ArgumentNullException(nameof(gameConfig),
                        "Elympics Game Config is missing. Please make sure you have an ElympicsGameConfig asset in your project and it is properly configured.");

                var sessionManager = GetComponent<SessionManager>();
                if (sessionManager == null)
                    throw new ArgumentNullException(nameof(sessionManager), $"Couldn't find {nameof(sessionManager)} component on gameObject {gameObject.name}");

                var launcher = ElympicsMatchLauncher.Instance;
#if UNITY_EDITOR || !UNITY_WEBGL || ELYMPICS_PLAYPAD_KEEP_LOBBY
                ReplayCommunicator = new WebGLExternalReplay(_playPadMessagingSystem, launcher, retriever => Lobby.WatchReplay(retriever));
#else
                ReplayCommunicator = new WebGLExternalReplay(_playPadMessagingSystem, launcher, retriever => WebGLExternalReplay.LaunchReplay(gameConfig.GameId, retriever));
#endif
                _communicatorInternal = new PlayPadCommunicatorInternal(gameConfig!.GameId, gameConfig.UseWeb, () => ElympicsMatchLauncher.Instance);

                var authFactory = new AuthFactory();
#if UNITY_EDITOR || !UNITY_WEBGL || ELYMPICS_PLAYPAD_KEEP_LOBBY
                authFactory.RegisterAuthProvider(Lobby, LaunchMode.Lobby | LaunchMode.Gameplay);
#endif
                authFactory.RegisterAuthProvider(_communicatorInternal, LaunchMode.Gameplay);
                sessionManager.Init(authFactory, _playPadMessagingSystem);
                SessionManager = sessionManager;

                _webGLFunctionalities = new WebGLFunctionalities(_playPadMessagingSystem);
                _heartbeat = new WebGLHeartbeatCommunicator(_playPadMessagingSystem);
                ExternalAuthenticator = UseMockAuth
                    ? mockConfiguration!.customAuthenticatorCommunicator
                    : new WebGLExternalAuthenticator(_playPadMessagingSystem, sessionManager, _heartbeat);
                TournamentCommunicator = UseMockTournament
                    ? mockConfiguration!.customTournamentCommunicator
                    : new WebGLTournamentCommunicator(VirtualDepositCommunicator!, _playPadMessagingSystem);
                GameStatusCommunicator = UseMockGameStatus
                    ? mockConfiguration!.customGameStatusCommunicator
#if UNITY_EDITOR || !UNITY_WEBGL || ELYMPICS_PLAYPAD_KEEP_LOBBY
                    : new WebGLGameStatusCommunicator(_playPadMessagingSystem, Lobby, TournamentCommunicator!, launcher);
#else
                    : new WebGLGameStatusCommunicator(_playPadMessagingSystem, launcher);
#endif
                ExternalUiCommunicator = UseMockExternalUi
                    ? mockConfiguration!.customExternalUiCommunicator
                    : new WebGLExternalUiCommunicator(_playPadMessagingSystem);
                LeaderboardCommunicator = UseMockLeaderboard
                    ? mockConfiguration!.customLeaderboardCommunicator
                    : new WebGLLeaderboardCommunicator(_playPadMessagingSystem);
                LobbyCommunicator = UseMockLobby ? mockConfiguration!.customLobbyExternalCommunicator : new WebGLExternalLobby(_playPadMessagingSystem, _communicatorInternal);
#if UNITY_EDITOR || !UNITY_WEBGL || ELYMPICS_PLAYPAD_KEEP_LOBBY
                Room.BeforeMarkYourselfReady = BeforeSetReady;
#endif
                ExternalWebCommunicator = new WebGLWebCommunicator(_playPadMessagingSystem);
                _sentry = new WebGLExternalSentryCommunicator(_playPadMessagingSystem, launcher);
                Instance = this;
            }
            else
                Destroy(gameObject);
        }

#if UNITY_EDITOR || !UNITY_WEBGL || ELYMPICS_PLAYPAD_KEEP_LOBBY
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
#endif

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
            _sentry?.Dispose();
        }

        private bool UseMockAuth => useMockConfiguration
            && mockConfiguration
            && mockConfiguration!.useStandaloneAuthenticationCommunicator
            && mockConfiguration!.customAuthenticatorCommunicator
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

        private bool UseMockLeaderboard => useMockConfiguration
            && mockConfiguration
            && mockConfiguration!.useCustomStandaloneLeaderboardCommunicator
            && mockConfiguration!.customLeaderboardCommunicator
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
