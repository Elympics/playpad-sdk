#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.Core.Logger;
using Elympics.Models.Authentication;
using ElympicsPlayPad.ExternalCommunicators;
using ElympicsPlayPad.ExternalCommunicators.Authentication;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Extensions;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;
using ElympicsPlayPad.ExternalCommunicators.GameStatus;
using ElympicsPlayPad.ExternalCommunicators.Leaderboard;
using ElympicsPlayPad.ExternalCommunicators.Lobby;
using ElympicsPlayPad.ExternalCommunicators.Tournament;
using ElympicsPlayPad.ExternalCommunicators.VirtualDeposit;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Session.Exceptions;
using ElympicsPlayPad.Session.Strategies;
using ElympicsPlayPad.Utility;
using ElympicsPlayPad.Utils;
using JetBrains.Annotations;
using SCS;
using UnityEngine;

namespace ElympicsPlayPad.Session
{
    [DefaultExecutionOrder(ElympicsLobbyExecutionOrders.SessionManager)]
    public class SessionManager : MonoBehaviour, ISessionManager
    {
        [PublicAPI]
        public event Action? StartSessionInfoUpdate;

        [PublicAPI]
        public event Action? FinishSessionInfoUpdate;

        [PublicAPI]
        public SessionInfo? CurrentSession { get; private set; }

        [PublicAPI]
        public bool ConnectedWithPlayPad => instance != null;

        [SerializeField] private string fallbackRegion = ElympicsRegions.Warsaw;

        private static SessionManager? instance;
        private string _region = null!;
        private ISessionManagerAuthProvider _authProvider = null!;
        private AuthFactory _authFactory = null!;
        private SessionManagerInitializationStrategy _initializationStrategy = null!;
        private static IExternalAuthenticator ExternalAuthenticator => PlayPadCommunicator.Instance!.ExternalAuthenticator!;
        private static IExternalGameStatusCommunicator GameStatusCommunicator => PlayPadCommunicator.Instance!.GameStatusCommunicator!;
        private static IExternalTournamentCommunicator TournamentCommunicator => PlayPadCommunicator.Instance!.TournamentCommunicator!;
        private static IExternalLeaderboardCommunicator LeaderboardCommunicator => PlayPadCommunicator.Instance!.LeaderboardCommunicator!;
        private static IExternalBlockChainCurrencyCommunicator? VirtualDepositCommunicator => PlayPadCommunicator.Instance!.VirtualDepositCommunicator;
        private static IExternalLobbyCommunicator LobbyCommunicator => PlayPadCommunicator.Instance!.LobbyCommunicator!;

        private readonly LoggerConfig _logger = ElympicsLogger.WithPlayPadSdkService()
            .WithClass(typeof(SessionManager))
            .WithMonitoringEnabled();

        private AuthData? _newAuthDataRequest;
        private string? _newRegionChange;
        private IPlayPadMessagingSystem _playpadMessagingSystem = null!;

        private CancellationTokenSource _sessionManagerToken = new();

        internal void Init(AuthFactory authFactory, IPlayPadMessagingSystem playPadCommunicator)
        {
            _authFactory = authFactory;
            _playpadMessagingSystem = playPadCommunicator;
        }

        private SessionManagerInitializationStrategy CreateInitializationStrategy(LaunchMode launchMode)
        {
            if (launchMode.HasOnlyGamePlay())
                return new SessionManagerPlatformInitializationStrategy(LobbyCommunicator, GameStatusCommunicator);
            if (launchMode.HasLobby())
                return new SessionManagerPlayPadInitializationStrategy(
                    GameStatusCommunicator,
                    TournamentCommunicator,
                    LeaderboardCommunicator,
                    VirtualDepositCommunicator);

            throw new SessionmanagerException($"Unknown launchMode: {launchMode}");
        }

        /// <summary>
        /// Request PlayPad for authentication.
        /// Initialize Tournament if available.
        /// </summary>
        [PublicAPI]
        public async UniTask AuthenticateFromExternalAndConnect()
        {
            var logger = _logger.WithMethodName();
            if (instance != null)
                throw new SessionmanagerException("Session Manager already initialized.");

            if (instance == null)
            {
                ExternalAuthenticator.AuthenticationUpdated += OnAuthDataChanged;
                ExternalAuthenticator.RegionUpdated += OnRegionUpdated;

                if (SmartContractService.Instance != null)
                    await SmartContractService.Instance.Initialize();
                await _playpadMessagingSystem.Connect();
                StartSessionInfoUpdate?.Invoke();
                var handshake = await SetupHandshake();
                ElympicsLogger.State.SetRegion(handshake.ClosestRegion);
                ElympicsLogger.State.SetPlayPad(ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js.PlayPadMessagingSystem.ProtocolVersion,
                    handshake.Capabilities.ToString(),
                    handshake.FeatureAccess.ToString());
                logger.LogInfo($"Handshake info received: IsMobile={handshake.IsMobile}, Environment={handshake.Environment}, LaunchMode={handshake.LaunchMode}");
                _initializationStrategy = CreateInitializationStrategy(handshake.LaunchMode);
                _authProvider = _authFactory.GetAuthProvider(handshake.LaunchMode);
                Debug.Log($"[CRITICAL] Auth provider: {_authProvider.GetType().FullName}");
                Debug.Log($"[CRITICAL] LaunchMode: {handshake.LaunchMode}, Int value: {(int)handshake.LaunchMode}");
                _region = await GetClosestRegion(handshake.ClosestRegion);
                var authData = await Authenticate();
#pragma warning disable CS0618 // Type or member is obsolete
                ElympicsLogger.State.SetUserId(authData.UserId);
                ElympicsLogger.State.SetAuthType(authData.AuthType);
                ElympicsLogger.State.SetNickname(authData.Nickname);
#pragma warning restore CS0618 // Type or member is obsolete
                logger.LogInfo($"Authentication succeeded: AuthType={authData.AuthType}, UserId={authData.UserId}, Nickname={authData.Nickname}");
                await _initializationStrategy.InitializePostAuthenticationAsync(handshake, authData, _sessionManagerToken.Token);
                SetupSession(handshake, _region, authData);
                FinishSessionInfoUpdate?.Invoke();
                instance = this;
                AuthChangeRequestDispatcher(_sessionManagerToken.Token).Forget(e => logger.LogException(e));
            }
            else
                Destroy(gameObject);
        }

        private async UniTask<HandshakeInfo> SetupHandshake()
        {
            var sdkVersion = ElympicsConfig.SdkVersion;
            var lobbyPackageVersion = PlayPadSdkVersionRetriever.GetVersionStringFromAssembly();
            var config = ElympicsConfig.LoadCurrentElympicsGameConfig()!;
            var gameName = config.GameName;
            var gameId = config.GameId;
            var versionName = config.GameVersion;
            return await ExternalAuthenticator.InitializationMessage(gameId, gameName, versionName, sdkVersion, lobbyPackageVersion);
        }

        private async UniTask<AuthData> Authenticate()
        {
            var logger = _logger.WithMethodName();
            var result = await ExternalAuthenticator.Authenticate() ?? throw logger.LogExceptionAndReturn(new SessionManagerAuthException($"External authenticator did not return AuthData."));
            try
            {
                await AuthWithCached(result, _region, false);
                return result;
            }
            catch (Exception e)
            {
                throw logger.LogExceptionAndReturn(new SessionManagerFatalError(e.Message));
            }
        }

        private void SetupSession(HandshakeInfo handshake, string region, AuthData authData)
        {
            var (accountWallet, signWallet, tonWallet) = WalletAddress.ExtractWalletAddresses(authData);
            CurrentSession = new SessionInfo(authData,
                accountWallet,
                signWallet,
                handshake.Capabilities,
                handshake.Environment,
                handshake.IsMobile,
                region,
                handshake.FeatureAccess,
                tonWallet,
                handshake.UserPrefs,
                handshake.LaunchMode);
        }

        private void SetupSession(AuthData authData, string region, SessionInfo currentSession)
        {
            ThrowIfCurrentSessionNull("Something went wrong. There should be existing current session");

            var (accountWallet, signWallet, tonWallet) = WalletAddress.ExtractWalletAddresses(authData);
            CurrentSession = new SessionInfo(authData,
                accountWallet,
                signWallet,
                currentSession.Capabilities,
                currentSession.Environment,
                currentSession.IsMobile,
                region,
                currentSession.Features,
                tonWallet,
                currentSession.UserPrefs,
                currentSession.LaunchMode);
        }

        private async UniTask<string> GetClosestRegion(string externalClosestRegion)
        {
            if (!string.IsNullOrEmpty(externalClosestRegion))
                return externalClosestRegion;

            Debug.LogWarning($"External closest region is null.");
            var closestRegion = await FindClosestRegion();
            if (!string.IsNullOrEmpty(closestRegion))
                return closestRegion;
            Debug.LogWarning($"Custom region search failed to find closest region. Using fallback region \"{fallbackRegion}\".");
            return fallbackRegion;
        }

        private static async UniTask<string> FindClosestRegion()
        {
            try
            {
                var availableRegions = await ElympicsRegions.GetAvailableRegions();
                if (availableRegions == null || availableRegions.Count == 0)
                    return string.Empty;

                var closestRegion = string.Empty;
                try
                {
                    (closestRegion, _) = await ElympicsCloudPing.ChooseClosestRegion(availableRegions);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                return closestRegion;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return string.Empty;
            }
        }

        private async UniTask AuthWithCached(AuthData cachedData, string region, bool autoRetry)
        {
            try
            {
                if (_authProvider.IsAuthenticated)
                    _authProvider.SignOut();

                Debug.Log($"CachedData is {cachedData.AuthType}.");
                await _authProvider.Authenticate(cachedData, region, autoRetry);
            }
            catch (Exception e)
            {
                throw new SessionManagerAuthException($"Couldn't login using cached data. Reason: {Environment.NewLine} {e.Message}");
            }
        }


        private void OnAuthDataChanged(AuthData data) => _newAuthDataRequest = data;

        private UniTask OnAuthChangedWithRegionAsync(AuthData authData, string newRegion)
        {
            _region = newRegion;
            return OnAuthDataChangedAsync(authData);
        }

        private async UniTask OnAuthDataChangedAsync(AuthData data)
        {
            StartSessionInfoUpdate?.Invoke();
            ThrowIfCurrentSessionNull("No initial authentication was performed. Can't re-authenticate.");
            try
            {
                await AuthWithCached(data, _region, false);
                SetupSession(data, _region, CurrentSession!.Value);
            }
            finally
            {
                FinishSessionInfoUpdate?.Invoke();
            }
        }

        private void OnRegionUpdated(string newRegion) => _newRegionChange = newRegion;

        private async UniTask OnRegionUpdatedAsync(string newRegion)
        {
            ThrowIfCurrentSessionNull("No initial authentication was performed. Can't re-authenticate.");

            _region = newRegion;
            StartSessionInfoUpdate?.Invoke();
            try
            {
                await AuthWithCached(CurrentSession!.Value.AuthData, _region, false);
                SetupSession(CurrentSession.Value.AuthData, _region, CurrentSession.Value);
            }
            finally
            {
                FinishSessionInfoUpdate?.Invoke();
            }
        }

        private async UniTask AuthChangeRequestDispatcher(CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                    return;

                try
                {
                    var currentAuthData = _newAuthDataRequest;
                    var currentRegion = _newRegionChange;
                    _newAuthDataRequest = null;
                    _newRegionChange = null;

                    if (currentAuthData != null && !string.IsNullOrEmpty(currentRegion))
                        await OnAuthChangedWithRegionAsync(currentAuthData, currentRegion!);
                    else if (currentAuthData != null)
                        await OnAuthDataChangedAsync(currentAuthData);
                    else if (!string.IsNullOrEmpty(currentRegion))
                        await OnRegionUpdatedAsync(currentRegion!);
                }
                catch (Exception e)
                {
                    var logger = _logger.WithMethodName();
                    logger.LogException(e);
                }
                finally
                {
                    await UniTask.Yield();
                }
            }
        }

        private void ThrowIfCurrentSessionNull(string message)
        {
            if (!CurrentSession.HasValue)
                throw new SessionmanagerException(message);
        }

        private void OnDestroy()
        {
            _sessionManagerToken.Cancel();
            ExternalAuthenticator.AuthenticationUpdated -= OnAuthDataChanged;
        }

        internal void Reset()
        {
            instance = null;
            CurrentSession = null;
            _authProvider?.SignOut();
        }

        internal static string PlayPadMessagingSystem => nameof(_playpadMessagingSystem);
    }
}
