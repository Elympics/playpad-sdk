#nullable enable
using System;
using System.Data;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.Models.Authentication;
using ElympicsPlayPad.ExternalCommunicators;
using ElympicsPlayPad.ExternalCommunicators.Authentication;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Extensions;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;
using ElympicsPlayPad.ExternalCommunicators.Web3.Wallet;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication;
using ElympicsPlayPad.JWT;
using ElympicsPlayPad.JWT.Extensions;
using ElympicsPlayPad.Session.Exceptions;
using ElympicsPlayPad.Tournament;
using ElympicsPlayPad.Utility;
using ElympicsPlayPad.Web3;
using ElympicsPlayPad.Web3.Data;
using ElympicsPlayPad.Wrappers;
using JetBrains.Annotations;
using SCS;
using UnityEngine;

namespace ElympicsPlayPad.Session
{
    [RequireComponent(typeof(Web3Wallet))]
    [DefaultExecutionOrder(ElympicsLobbyExecutionOrders.SessionManager)]
    public class SessionManager : MonoBehaviour
    {
        [PublicAPI]
        public SessionInfo? CurrentSession { get; private set; }

        [PublicAPI]
        public IElympicsTournament? ElympicsTournament;

        [SerializeField] private string fallbackRegion = ElympicsRegions.Warsaw;

        private static SessionManager? instance;
        private string _region = null!;
        private IElympicsLobbyWrapper _lobbyWrapper = null!;
        private Web3Wallet? _wallet;
        private static IExternalAuthenticator ExternalAuthenticator => PlayPadCommunicator.Instance!.ExternalAuthenticator!;
        private IExternalWalletCommunicator WalletCommunicator => PlayPadCommunicator.Instance!.WalletCommunicator!;
        private WalletConnectionStatus? _walletConnectionUpdate;

        private void Start()
        {
            ElympicsTournament = GetComponent<IElympicsTournament>();
            _lobbyWrapper = GetComponent<IElympicsLobbyWrapper>();
            _wallet = GetComponent<Web3Wallet>();
            _wallet.WalletConnectionUpdatedInternal += OnWalletConnectionUpdated;
            PlayPadCommunicator.Instance!.AuthDataChanged += OnAuthDataChanged;
        }

        /// <summary>
        /// Request PlayPad for authentication data and initialize ElympicsTournament when tournament is active.
        /// </summary>
        [PublicAPI]
        public async UniTask AuthenticateFromExternalAndConnect()
        {
            if (instance == null)
            {
                if (SmartContractService.Instance != null)
                    await SmartContractService.Instance.Initialize();

                await TryCheckExternalAuthentication();
                if (ElympicsTournament != null
                    && CurrentSession!.Value.TournamentInfo.HasValue)
                    await ElympicsTournament.Initialize(CurrentSession.Value.TournamentInfo.Value);
                instance = this;
            }
            else
                Destroy(gameObject);
        }

        [PublicAPI]
        public async UniTask<bool> TryReAuthenticateIfAuthDataChanged()
        {
            if (instance == null)
                throw new Exception($"Please Initialize SessionManager using {nameof(AuthenticateFromExternalAndConnect)} method");

            if (IsWalletEligible() is false)
                return false;

            Debug.Log($"[{nameof(SessionManager)}] Check if wallet connection has changed.");
            switch (_walletConnectionUpdate)
            {
                case WalletConnectionStatus.Connected:
                    Debug.Log($"[{nameof(SessionManager)}] Already authenticated but wallet address has changed.");
                    _walletConnectionUpdate = null;
                    await AuthWithWalletFromCacheOrNew();
                    return true;
                case WalletConnectionStatus.Disconnected:
                    Debug.Log($"[{nameof(SessionManager)}] Already authenticated but wallet has been disconnected.");
                    _walletConnectionUpdate = null;
                    await AnonymousAuthentication();
                    return true;
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return false;
        }

        [PublicAPI]
        public void ShowExternalWalletConnectionPanel() => _wallet!.ExternalShowConnectToWallet();

        [PublicAPI]
        public void SelectChain() => _wallet!.ExternalShowChainSelection();

        [PublicAPI]
        public async UniTask ConnectToWallet()
        {
            try
            {
                var address = await CheckWalletConnection();
                if (string.IsNullOrEmpty(address))
                    throw new WalletConnectionException($"Wallet has to be connected. Use {nameof(ShowExternalWalletConnectionPanel)}.");

                await TryConnectToWalletOrAnonymous(address);

            }
            catch (ResponseException)
            {
                throw new WalletConnectionException($"Wallet has to be connected. Use {nameof(ShowExternalWalletConnectionPanel)}.");
            }
            catch (ChainIdMismatch chainIdMismatch)
            {
                throw new WalletConnectionException($"{chainIdMismatch.Message}. Use {nameof(SelectChain)}.");
            }
        }

        private async UniTask TryConnectToWalletOrAnonymous(string walletAddress)
        {
            if (string.IsNullOrEmpty(walletAddress) is false)
                await AuthWithWalletFromCacheOrNew();
            else
                await AnonymousAuthentication();
        }

        private async UniTask TryCheckExternalAuthentication()
        {
            Debug.Log($"{nameof(SessionManager)} Check external authentication.");
            var sdkVersion = ElympicsConfig.SdkVersion;
            var lobbyPackageVersion = PlayPadSdkVersionRetriever.GetVersionStringFromAssembly();
            var config = ElympicsConfig.LoadCurrentElympicsGameConfig();
            var gameName = config.GameName;
            var gameId = config.GameId;
            var versionName = config.GameVersion;
#if UNITY_EDITOR || !UNITY_WEBGL
            if (ExternalAuthenticator is null)
                throw new NoNullAllowedException($"Please provide custom external authorizer via {nameof(PlayPadCommunicator.SetCustomExternalAuthenticator)}");
#endif
            var result = await ExternalAuthenticator.InitializationMessage(gameId, gameName, versionName, sdkVersion, lobbyPackageVersion);
            await SetClosestRegion(result.ClosestRegion);
#if UNITY_EDITOR
            var standaloneAuthType = result.AuthData.AuthType;
            if (standaloneAuthType == AuthType.EthAddress)
            {
                await _wallet!.ConnectWeb3();
            }
            await _lobbyWrapper.ConnectToElympicsAsync(new ConnectionData()
            {
                AuthType = standaloneAuthType,
                Region = new RegionData(_region)
            });
            var unityPayload = _lobbyWrapper.AuthData.JwtToken.ExtractUnityPayloadFromJwt();
            var (accountWallet, signWallet) = GetAccountAndSignWalletAddressesFromPayload(unityPayload, standaloneAuthType);
            CurrentSession = new SessionInfo(_lobbyWrapper.AuthData, accountWallet, signWallet, result.Capabilities, result.Environment, result.IsMobile, _region, result.TournamentInfo);
            return;
#endif
            try
            {
                await AuthWithCached(result.AuthData, false, result);
            }
            catch (Exception e)
            {
                throw new SessionManagerFatalError(e.Message);
            }
        }
        private async UniTask SetClosestRegion(string externalClosestRegion)
        {
            if (string.IsNullOrEmpty(externalClosestRegion))
            {
                Debug.LogWarning($"External closest region is null.");
                var closestRegion = await FindClosestRegion();
                if (string.IsNullOrEmpty(closestRegion))
                {
                    Debug.LogWarning($"Custom region search failed to find closest region. Using fallback region \"{fallbackRegion}\".");
                    _region = fallbackRegion;
                }
                else
                    _region = closestRegion;
            }
            else
                _region = externalClosestRegion;

            Debug.Log($"Closest region is \"{_region}\"");
        }

        private static async UniTask<string> FindClosestRegion()
        {
            try
            {
                var availableRegions = await ElympicsRegions.GetAvailableRegions();
                if (availableRegions == null
                    || availableRegions.Count == 0)
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

        private async UniTask AuthWithWalletFromCacheOrNew()
        {
            try
            {
                if (_lobbyWrapper.IsAuthenticated)
                {
                    _lobbyWrapper.SignOut();
                }
                await AuthWithEth();
                //var savedAuthData = _authDataStorage.Get();
                // if (savedAuthData == null
                //     || savedAuthData.AuthType == AuthType.ClientSecret)
                // {
                //     await AuthWithEth();
                // }
                // else
                // {
                //     var tokenAsString = JsonWebToken.Decode(savedAuthData.JwtToken, "", false);
                //     if (string.IsNullOrEmpty(tokenAsString))
                //     {
                //         Debug.Log("[SessionManager] found token was invalid. Forcing re-authentication.");
                //         await AuthWithEth();
                //         return;
                //     }
                //     var token = JsonConvert.DeserializeObject<ElympicsJwt>(tokenAsString);
                //     var isValid = IsTokenValid(token);
                //     var isMatching = IsTokenMatching(token, _wallet!.Address);
                //     if (!isValid
                //         || !isMatching)
                //     {
                //         Debug.Log("[SessionManager] found token was invalid. Forcing re-authentication.");
                //         await AuthWithEth();
                //         return;
                //     }
                //
                //     Debug.Log("[SessionManager] found matching cached token. Reusing value confirmed!");
                //     await AuthWithCached(savedAuthData, true, null);
                //     //SaveNewAuthData();
                // }
            }
            catch (SessionManagerAuthException)
            {
                await AnonymousAuthentication();
            }
            await UniTask.CompletedTask;
        }
        // private void SaveNewAuthData()
        // {
        //     var authData = _lobbyWrapper.AuthData;
        //
        //     if (authData is null)
        //     {
        //         _authDataStorage.Clear();
        //     }
        //     else
        //     {
        //         _authDataStorage.Set(authData);
        //     }
        // }

        private void OnWalletConnectionUpdated(WalletConnectionStatus status)
        {
            Debug.Log($"Wallet connection status changed to: {status}");
            _walletConnectionUpdate = status;
        }

        private bool IsTokenValid(ElympicsJwt token)
        {
            var gameConfig = ElympicsConfig.LoadCurrentElympicsGameConfig();
            if (gameConfig == null)
                return false;
            return token.gameId == gameConfig.GameId && token.gameName == gameConfig.GameName && token.versionName == gameConfig.GameVersion && token.chainId == _wallet!.ChainId && token.expiry >= DateTime.Now.AddHours(-1);
        }

        private bool IsTokenMatching(ElympicsJwt token, string expectedAddress)
        {
            return token.EthAddress.ToLower() == expectedAddress.ToLower();
        }

        private async UniTask AuthWithCached(AuthData cachedData, bool autoRetry, ExternalAuthData? external)
        {
            try
            {
                Debug.Log($"CachedData is {cachedData.AuthType}.");
                await _lobbyWrapper.ConnectToElympicsAsync(new ConnectionData()
                {
                    Region = new RegionData(_region),
                    AuthFromCacheData = new CachedAuthData(cachedData, autoRetry)
                });

                // string? accountWallet = null;
                // string? signWallet = null;

                var payloadDeserialized = cachedData.JwtToken.ExtractUnityPayloadFromJwt();
                var (accountWallet, signWallet) = GetAccountAndSignWalletAddressesFromPayload(payloadDeserialized, cachedData.AuthType);
                var capa = external?.Capabilities ?? CurrentSession!.Value.Capabilities;
                var enviro = external?.Environment ?? CurrentSession!.Value.Environment;
                var isMobile = external?.IsMobile ?? CurrentSession!.Value.IsMobile;
                var closestRegion = external?.ClosestRegion ?? CurrentSession!.Value.ClosestRegion;
                CurrentSession = new SessionInfo(_lobbyWrapper.AuthData, accountWallet, signWallet, capa, enviro, isMobile, closestRegion, external?.TournamentInfo);
            }
            catch (Exception e)
            {
                throw new SessionManagerAuthException($"Couldn't login using cached data. Logging as guest. Reason: {Environment.NewLine} {e.Message}");
            }
        }

        private Tuple<string?, string?> GetAccountAndSignWalletAddressesFromPayload(JwtPayload payload, AuthType currentAuthType)
        {
            var accountWallet = payload.ethAddress is not null ? payload.ethAddress : null;
            string? signWallet = null;
            if (currentAuthType.IsWallet())
                signWallet = accountWallet;
            return new Tuple<string?, string?>(accountWallet, signWallet);
        }

        private async UniTask AuthWithEth()
        {
            Debug.Log($"[{nameof(SessionManager)}] EthAddress Auth.");
            try
            {
                await _lobbyWrapper.ConnectToElympicsAsync(new ConnectionData()
                {
                    AuthType = AuthType.EthAddress,
                    Region = new RegionData(_region)
                });
                CurrentSession = new SessionInfo(_lobbyWrapper.AuthData!, _wallet.Address, _wallet.Address, CurrentSession.Value.Capabilities, CurrentSession.Value.Environment, CurrentSession.Value.IsMobile, CurrentSession.Value.ClosestRegion, CurrentSession.Value.TournamentInfo);
                //SaveNewAuthData();
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
                throw new SessionManagerAuthException("Couldn't authenticate with eth.");
            }
        }

        private async UniTask AnonymousAuthentication()
        {
            if (_lobbyWrapper.AuthData?.AuthType is AuthType.ClientSecret)
            {
                Debug.Log($"Already authenticated as {AuthType.ClientSecret}.");
                return;
            }
            try
            {
                if (_lobbyWrapper.IsAuthenticated)
                {
                    _lobbyWrapper.SignOut();
                }
                Debug.Log($"[{nameof(SessionManager)}] ClientSecret Auth.");
                await _lobbyWrapper.ConnectToElympicsAsync(new ConnectionData()
                {
                    AuthType = AuthType.ClientSecret,
                    Region = new RegionData(_region)
                });
                CurrentSession = new SessionInfo(_lobbyWrapper.AuthData, null, null, CurrentSession!.Value.Capabilities, CurrentSession.Value.Environment, CurrentSession.Value.IsMobile, CurrentSession.Value.ClosestRegion, CurrentSession.Value.TournamentInfo);
            }
            catch (Exception)
            {
                CurrentSession = null;

                if (!_lobbyWrapper.IsAuthenticated)
                    throw;

                _lobbyWrapper.SignOut();
                throw;
            }
        }

        private async UniTask<string> CheckWalletConnection()
        {
            if (IsWalletEligible() is false)
                return string.Empty;

            return await _wallet!.ConnectWeb3();
        }

        private void OnAuthDataChanged(AuthData data) => AuthWithCached(data, false, null).Forget();

        private void OnDestroy()
        {
            if (_wallet != null)
                _wallet.WalletConnectionUpdatedInternal -= OnWalletConnectionUpdated;
            PlayPadCommunicator.Instance!.AuthDataChanged -= OnAuthDataChanged;
            ExternalAuthenticator.Dispose();
        }
        private bool IsWalletEligible() => CurrentSession.HasValue && (CurrentSession.Value.Capabilities.IsEth() || CurrentSession.Value.Capabilities.IsTon());

        internal void Reset()
        {
            instance = null;
            CurrentSession = null;
            //_authDataStorage.Clear();
            _lobbyWrapper.SignOut();
        }
    }
}
