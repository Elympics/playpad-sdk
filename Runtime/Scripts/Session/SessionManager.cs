#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.ElympicsSystems.Internal;
using Elympics.Models.Authentication;
using ElympicsPlayPad.ExternalCommunicators;
using ElympicsPlayPad.ExternalCommunicators.Authentication;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Extensions;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;
using ElympicsPlayPad.ExternalCommunicators.GameStatus;
using ElympicsPlayPad.ExternalCommunicators.Leaderboard;
using ElympicsPlayPad.ExternalCommunicators.Tournament;
using ElympicsPlayPad.ExternalCommunicators.VirtualDeposit;
using ElympicsPlayPad.JWT;
using ElympicsPlayPad.JWT.Extensions;
using ElympicsPlayPad.Session.Exceptions;
using ElympicsPlayPad.Utility;
using ElympicsPlayPad.Wrappers;
using JetBrains.Annotations;
using SCS;
using UnityEngine;

namespace ElympicsPlayPad.Session
{
    [DefaultExecutionOrder(ElympicsLobbyExecutionOrders.SessionManager)]
    public class SessionManager : MonoBehaviour
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
        private IElympicsLobbyWrapper _lobbyWrapper = null!;
        private static IExternalAuthenticator ExternalAuthenticator => PlayPadCommunicator.Instance!.ExternalAuthenticator!;
        private static IExternalGameStatusCommunicator GameStatusCommunicator => PlayPadCommunicator.Instance!.GameStatusCommunicator!;
        private static IExternalTournamentCommunicator TournamentCommunicator => PlayPadCommunicator.Instance!.TournamentCommunicator!;
        private static IExternalLeaderboardCommunicator LeaderboardCommunicator => PlayPadCommunicator.Instance!.LeaderboardCommunicator!;
        private static IExternalBlockChainCurrencyCommunicator? VirtualDepositCommunicator => PlayPadCommunicator.Instance!.VirtualDepositCommunicator;

        private ElympicsLoggerContext _logger;

        private AuthData? _newAuthDataRequest;
        private string? _newRegionChange;


        private CancellationTokenSource _sessionManagerToken = new();


        internal void Init(ElympicsLoggerContext logger)
        {
            _lobbyWrapper = GetComponent<IElympicsLobbyWrapper>();
            _logger = logger.WithContext(nameof(SessionManager));
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

                StartSessionInfoUpdate?.Invoke();
                var handshake = await SetupHandshake();
                _ = logger.SetRegion(handshake.ClosestRegion).SetFeatureAccess(handshake.FeatureAccess.ToString()).SetCapabilities(handshake.Capabilities.ToString());
                _region = await GetClosestRegion(handshake.ClosestRegion);
                var authData = await Authenticate();
                var (accountWallet, signWallet, _) = ExtractWalletAddresses(authData);
#pragma warning disable CS0618 // Type or member is obsolete
                _ = logger.SetAuthType(authData.AuthType).SetUserId(authData.UserId.ToString()).SetNickname(authData.Nickname)
#pragma warning restore CS0618 // Type or member is obsolete
                    .SetWalletAddress(signWallet ?? accountWallet ?? string.Empty);
                if (handshake.FeatureAccess.HasTournament())
                {
                    var tournament = await TournamentCommunicator.GetTournament();
                    _ = logger.SetTournamentId(tournament?.Id);
                }

                _ = await GameStatusCommunicator.CanPlayGame(false);

                if (VirtualDepositCommunicator != null)
                    _ = await VirtualDepositCommunicator.GetElympicsCoins();

                if (handshake.FeatureAccess.HasLeaderboard())
                    _ = await LeaderboardCommunicator.FetchLeaderboard();

                if (handshake.FeatureAccess.HasUserHighScore())
                    _ = await LeaderboardCommunicator.FetchUserHighScore();

                if (handshake.FeatureAccess.HasVirtualDeposit())
                    if (VirtualDepositCommunicator != null)
                        _ = await VirtualDepositCommunicator.GetVirtualDeposit();

                SetupSession(handshake, _region, authData);
                FinishSessionInfoUpdate?.Invoke();
                instance = this;
                AuthChangeRequestDispatcher(_sessionManagerToken.Token).Forget(logger.Exception);
                logger.Log($"PlayPad connection established.");
            }
            else
                Destroy(gameObject);
        }

        private async UniTask<HandshakeInfo> SetupHandshake()
        {
            var sdkVersion = ElympicsConfig.SdkVersion;
            var lobbyPackageVersion = PlayPadSdkVersionRetriever.GetVersionStringFromAssembly();
            var config = ElympicsConfig.LoadCurrentElympicsGameConfig();
            var gameName = config.GameName;
            var gameId = config.GameId;
            var versionName = config.GameVersion;
            return await ExternalAuthenticator.InitializationMessage(gameId, gameName, versionName, sdkVersion, lobbyPackageVersion);
        }

        private async UniTask<AuthData> Authenticate()
        {
            var logger = _logger.WithMethodName();
            var result = await ExternalAuthenticator.Authenticate() ?? throw logger.CaptureAndThrow(new SessionManagerAuthException($"External authenticator did not return AuthData."));
#if UNITY_EDITOR || ELYMPICS_DISABLE_PLAYPAD
            var standaloneAuthType = result.AuthType;
            if (standaloneAuthType != AuthType.ClientSecret)
                throw new SessionManagerAuthException($"Cannot authenticate with {standaloneAuthType} on Editor or with ELYMPICS_DISABLE_PLAYPAD. Please use {AuthType.ClientSecret}");

            await _lobbyWrapper.ConnectStandaloneEditorToElympicsAsync(result, _region);

            return _lobbyWrapper.AuthData!;
#else
            try
            {
                await AuthWithCached(result, _region, false);
                return result;
            }
            catch (Exception e)
            {
                throw logger.CaptureAndThrow(new SessionManagerFatalError(e.Message));
            }
#endif
        }

        private void SetupSession(HandshakeInfo handshake, string region, AuthData authData)
        {
            var (accountWallet, signWallet, tonWallet) = ExtractWalletAddresses(authData);
            CurrentSession = new SessionInfo(authData, accountWallet, signWallet, handshake.Capabilities, handshake.Environment, handshake.IsMobile, region, handshake.FeatureAccess, tonWallet);
        }

        private void SetupSession(AuthData authData, string region, SessionInfo currentSession)
        {
            ThrowIfCurrentSessionNull("Something went wrong. There should be existing current session");

            var (accountWallet, signWallet, tonWallet) = ExtractWalletAddresses(authData);
            CurrentSession = new SessionInfo(authData,
                accountWallet,
                signWallet,
                currentSession.Capabilities,
                currentSession.Environment,
                currentSession.IsMobile,
                region,
                currentSession.Features,
                tonWallet);
        }

        private static (string? accountWallet, string? signWallet, string? tonWallet) ExtractWalletAddresses(AuthData authData)
        {
            var jwtPayload = authData.JwtToken.ExtractUnityPayloadFromJwt();
            var (accountWallet, signWallet, tonWallet) = GetAccountAndSignWalletAddressesFromPayload(jwtPayload, authData.AuthType);
            return (accountWallet, signWallet, tonWallet);
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
                    var result = await ElympicsCloudPing.ChooseClosestRegion(availableRegions);
                    closestRegion = result.Region;
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
                if (_lobbyWrapper.IsAuthenticated)
                    _lobbyWrapper.SignOut();

                Debug.Log($"CachedData is {cachedData.AuthType}.");
                await _lobbyWrapper.ConnectToElympicsAsync(new ConnectionData()
                {
                    Region = new RegionData(region),
                    AuthFromCacheData = new CachedAuthData(cachedData, autoRetry)
                });
            }
            catch (Exception e)
            {
                throw new SessionManagerAuthException($"Couldn't login using cached data. Reason: {Environment.NewLine} {e.Message}");
            }
        }

        private static (string? accountWallet, string? signWallet, string? tonWallet) GetAccountAndSignWalletAddressesFromPayload(JwtPayload payload, AuthType currentAuthType)
        {
            var accountWallet = payload.ethAddress;
            string? signWallet = null;
            if (currentAuthType.IsWallet())
                signWallet = accountWallet;
            var tonAddress = payload.tonNoBounceAddress ?? payload.tonAddress;
            return (accountWallet, signWallet, tonAddress);
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

                    if (currentAuthData != null && string.IsNullOrEmpty(currentRegion) is false)
                        await OnAuthChangedWithRegionAsync(currentAuthData, currentRegion!);
                    else if (currentAuthData != null)
                        await OnAuthDataChangedAsync(currentAuthData);
                    else if (string.IsNullOrEmpty(currentRegion) is false)
                        await OnRegionUpdatedAsync(currentRegion!);
                }
                catch (Exception e)
                {
                    var logger = _logger.WithMethodName();
                    logger.Exception(e);
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
            _lobbyWrapper.SignOut();
        }
    }
}
