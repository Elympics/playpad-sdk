#if UNITY_EDITOR || !UNITY_WEBGL || ELYMPICS_PLAYPAD_KEEP_LOBBY
#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.AssemblyCommunicator;
using Elympics.AssemblyCommunicator.Events;
using Elympics.Core.Logger;
using Elympics.Models.Authentication;
using Elympics.SnapshotAnalysis.Retrievers;
using Elympics.Util;
using ElympicsPlayPad.Auth;
using ElympicsPlayPad.Auth.Models;
using ElympicsPlayPad.Lobby;
using ElympicsPlayPad.Lobby.InternalModels;
using ElympicsPlayPad.Lobby.InternalModels.FromLobby;
using ElympicsPlayPad.Lobby.InternalModels.ToLobby;
using ElympicsPlayPad.Rooms;
using ElympicsPlayPad.Rooms.Mappers;
using ElympicsPlayPad.Wrappers.Connection;
using MatchmakingState = ElympicsPlayPad.Rooms.Models.MatchmakingState;

namespace ElympicsPlayPad.Wrappers
{
    /// <summary>
    /// Authentication, the lobby WebSocket and Rooms of <see cref="DefaultElympicsLobbyWrapper"/>. Matches found in Rooms and replays
    /// are launched through <see cref="IElympicsMatchLauncher"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="State"/> combines the connection state with the wrapper's own launches: while a match or replay launched here runs,
    /// it is <see cref="ElympicsState.PlayingMatch"/> or <see cref="ElympicsState.WatchReplay"/>, and connection changes are applied
    /// when the launcher returns to <see cref="MatchLauncherState.Idle"/>.
    /// </remarks>
    internal sealed class LobbyClient : IMatchLauncher, ICoinSource, IWebSocketSessionController, IDisposable
    {
        internal delegate UniTask<ElympicsLaunchResult> ReplayLauncher(string gameId, SnapshotAnalysisRetriever retriever, CancellationToken ct);

        public event Action<ElympicsConnectionData>? ElympicsConnectionEstablished;
        public event Action<ElympicsConnectionLostData>? ElympicsConnectionLost;

        public ElympicsState State { get; private set; } = ElympicsState.Disconnected;

        public AuthData? AuthData { get; private set; }
        public ElympicsUser? ElympicsUser { get; private set; }
        public bool IsAuthenticated => AuthData != null;
        public string CurrentRegion { get; private set; } = string.Empty;
        public IReadOnlyCollection<string>? AvailableRegions { get; private set; }
        public IReadOnlyCollection<CoinInfo>? AvailableCoins { get; private set; }
        public bool ShouldLoadGameplaySceneAfterMatchmaking { get; set; } = true;
        public bool IsCurrentlyInMatch => _launcher.IsCurrentlyInMatch;

        public WebSocketSession WebSocketSession { get; }
        public IRoomsManager RoomsManager { get; }

        private enum LaunchKind
        {
            None,
            Match,
            Replay,
        }

        private ElympicsState _connectionState = ElympicsState.Disconnected;
        private LaunchKind _launch = LaunchKind.None;
        private bool _connecting;
        private bool _reconnecting;
        private bool _matchmakingCancelRequested;

        private readonly IAuthClient _auth;
        private readonly string _clientSecret;
        private readonly IAvailableRegionRetriever _regionRetriever;
        private readonly IElympicsMatchLauncher _launcher;
        private readonly ReplayLauncher _launchReplay;
        private readonly ElympicsGameConfig _gameConfig;
        private readonly bool _useWeb;
        private readonly string _lobbyUrl;
        private SessionConnectionFactory? _sessionConnectionFactory;

        private readonly LoggerConfig _logger = ElympicsLogger.WithPlayPadSdkService()
            .WithClass(typeof(LobbyClient))
            .WithMonitoringEnabled();

        public LobbyClient(
            IAuthClient auth,
            string clientSecret,
            IAvailableRegionRetriever regionRetriever,
            IElympicsMatchLauncher launcher,
            ReplayLauncher launchReplay,
            ElympicsGameConfig gameConfig,
            bool useWeb,
            string lobbyUrl,
            Func<IWebSocketSessionController, WebSocketSession> createSession)
        {
            _auth = auth;
            _clientSecret = clientSecret;
            _regionRetriever = regionRetriever;
            _launcher = launcher;
            _launchReplay = launchReplay;
            _gameConfig = gameConfig;
            _useWeb = useWeb;
            _lobbyUrl = lobbyUrl;
            WebSocketSession = createSession(this);
            var roomsClient = new RoomsClient
            {
                Session = WebSocketSession,
                Coins = this,
            };
            RoomsManager = new RoomsManager(this, roomsClient);
            RoomsManager.Reset();
            _launcher.StateChanged += OnLauncherStateChanged;
        }

        #region Connection

        public async UniTask ConnectToElympicsAsync(ConnectionData data)
        {
            var logger = _logger.WithMethodName();
            try
            {
                switch (_connectionState)
                {
                    case ElympicsState.Disconnected:
                    case ElympicsState.Connected:
                        ThrowIfLaunched(nameof(ConnectToElympicsAsync));
                        await ConnectAsync(data);
                        break;
                    case ElympicsState.Connecting:
                        throw new ElympicsException("Already connecting to Elympics.");
                    case ElympicsState.Reconnecting:
                        logger.LogWarning(WarningMessage(nameof(ConnectToElympicsAsync)));
                        break;
                    case ElympicsState.Matchmaking:
                    case ElympicsState.PlayingMatch:
                    case ElympicsState.WatchReplay:
                    default:
                        throw new ElympicsException(ErrorMessage(nameof(ConnectToElympicsAsync)));
                }
            }
            catch (Exception e)
            {
                AuthData = null;
                throw logger.LogExceptionAndReturn(e);
            }
        }

        private async UniTask ConnectAsync(ConnectionData data)
        {
            if (_connecting)
                throw new ElympicsException("Already connecting to Elympics.");
            _connecting = true;
            SetConnectionState(ElympicsState.Connecting);
            try
            {
                CheckConnectionDataOrThrow(data);
                await Authorize(data);
                await FetchAvailableRegions();
                var gameData = await ConnectToLobby(data);
                if (gameData is not null)
                    await InitializeBasedOnGameData(gameData);
                await GetElympicsUserData();
                SetConnectionState(RoomsManager.CurrentRoom?.State.MatchmakingData?.MatchmakingState is MatchmakingState.Matchmaking or MatchmakingState.RequestingMatchmaking
                    ? ElympicsState.Matchmaking
                    : ElympicsState.Connected);
                OnSuccessfullyConnected(false);
            }
            catch (Exception)
            {
                SetConnectionState(ElympicsState.Disconnected);
                throw;
            }
            finally
            {
                _connecting = false;
            }
        }

        public void SignOut()
        {
            switch (_connectionState)
            {
                case ElympicsState.Connected when _launch == LaunchKind.None:
                    SignOutInternal();
                    SetConnectionState(ElympicsState.Disconnected);
                    break;
                case ElympicsState.Disconnected:
                    _logger.WithMethodName().LogWarning(WarningMessage(nameof(SignOut)));
                    break;
                case ElympicsState.Connected:
                case ElympicsState.Connecting:
                case ElympicsState.Matchmaking:
                case ElympicsState.PlayingMatch:
                case ElympicsState.WatchReplay:
                case ElympicsState.Reconnecting:
                default:
                    throw new ElympicsException(ErrorMessage(nameof(SignOut)));
            }
        }

        public async UniTask ReconnectIfPossible(DisconnectionData reason)
        {
            RoomsManager.Reset();
            if (reason.Reason != DisconnectionReason.Timeout)
            {
                Disconnect();
                ElympicsConnectionLost?.Invoke(new ElympicsConnectionLostData { DisconnectionData = reason });
                return;
            }

            await Reconnect(new ConnectionData
            {
                Region = new RegionData(CurrentRegion),
                AuthFromCacheData = AuthData != null ? new CachedAuthData(AuthData, false) : null,
            });
            if (_connectionState == ElympicsState.Disconnected)
                ElympicsConnectionLost?.Invoke(new ElympicsConnectionLostData
                {
                    DisconnectionData = reason.Reason == DisconnectionReason.Reconnection ? new DisconnectionData(DisconnectionReason.Timeout) : reason,
                });
        }

        private void Disconnect()
        {
            if (_connectionState is ElympicsState.Connected or ElympicsState.Matchmaking)
                SetConnectionState(ElympicsState.Disconnected);
        }

        internal async UniTask Reconnect(ConnectionData reconnectionData)
        {
            switch (_connectionState)
            {
                case ElympicsState.Connected:
                case ElympicsState.Matchmaking:
                    break;
                case ElympicsState.Disconnected:
                case ElympicsState.Reconnecting:
                    return;
                case ElympicsState.Connecting:
                case ElympicsState.PlayingMatch:
                case ElympicsState.WatchReplay:
                default:
                    throw new ElympicsException(ErrorMessage(nameof(Reconnect)));
            }
            if (_reconnecting)
                return;
            _reconnecting = true;
            _matchmakingCancelRequested = false;
            SetConnectionState(ElympicsState.Reconnecting);
            var logger = _logger.WithMethodName();
            try
            {
                logger.LogInfo("Trying to reconnect.");
                CheckConnectionDataOrThrow(reconnectionData);
                await Authorize(reconnectionData);
                var gameData = await ConnectToLobby(reconnectionData);
                if (gameData is not null)
                    await InitializeBasedOnGameData(gameData);
                await GetElympicsUserData();
            }
            catch (Exception e)
            {
                logger.LogWarning($"Failed to reconnect: {e.Message}");
                logger.LogError("Failed to reconnect to Elympics.");
                SignOutInternal();
                SetConnectionState(ElympicsState.Disconnected);
                return;
            }
            finally
            {
                _reconnecting = false;
            }

            SetConnectionState(RoomsManager.CurrentRoom?.IsDuringMatchmaking() is true ? ElympicsState.Matchmaking : ElympicsState.Connected);
            OnSuccessfullyConnected(true);
        }

        private static void CheckConnectionDataOrThrow(ConnectionData data)
        {
            if (data.AuthFromCacheData is null && data.AuthType is null && data.Region == null)
                // ReSharper disable once NotResolvedInText
                throw new ArgumentNullException(nameof(data), "All data parameters are null");
        }

        private async UniTask Authorize(ConnectionData data)
        {
            AuthorizationStrategy strategy = AuthData is not null
                ? new AuthorizedStrategy(AuthData, _auth, _clientSecret)
                : new UnauthorizedStrategy(_auth, _clientSecret);
            var result = await strategy.Authorize(data);
            if (result.IsFailure)
                throw new ElympicsException($"Authentication failed {result.Error}");
            if (result.Value == null)
                return;
            AuthData = result.Value;
            ElympicsLogger.State.SetUserId(AuthData.UserId);
            ElympicsLogger.State.SetAuthType(AuthData.AuthType);
            _logger.WithMethodName().LogInfo("Authentication completed.");
        }

        private async UniTask FetchAvailableRegions()
        {
            var availableRegions = await _regionRetriever.GetAvailableRegions();
            _sessionConnectionFactory = new SessionConnectionFactory(new StandardRegionValidator(availableRegions));
            AvailableRegions = availableRegions;
        }

        /// <returns>Server-side game details (only if <paramref name="data"/> or <see cref="AuthData"/> changed since last call).</returns>
        private async UniTask<GameDataResponseDto?> ConnectToLobby(ConnectionData data)
        {
            if (_sessionConnectionFactory == null)
                await FetchAvailableRegions();
            ConnectionStrategy strategy = (AuthData is not null, WebSocketSession.IsConnected) switch
            {
                (true, true) => new AuthorizedConnectedSocketConnectionStrategy(WebSocketSession, WebSocketSession.ConnectionDetails!.Value),
                (true, false) => new AuthorizedNotConnectedStrategy(WebSocketSession),
                (false, _) => new UnauthorizedSocketConnectionStrategy(WebSocketSession),
            };
            var connectionDetails = _sessionConnectionFactory!.CreateSessionConnectionDetails(_lobbyUrl, AuthData!, _gameConfig, data.Region);
            var gameData = await strategy.Connect(connectionDetails);
            CurrentRegion = connectionDetails.RegionName;
            return gameData;
        }

        private async UniTask InitializeBasedOnGameData(GameDataResponseDto gameDataResponse)
        {
            ElympicsLogger.State.SetFleetName(gameDataResponse.FleetName);
            ElympicsLogger.State.SetGameVersionId(gameDataResponse.GameVersionId);
            var coins = new List<CoinInfo>(gameDataResponse.CoinData.Count);
            foreach (var coin in gameDataResponse.CoinData)
                try
                {
                    coins.Add(await coin.Map().ToCoinInfo(_logger));
                }
                catch (ArgumentOutOfRangeException exc)
                {
                    ElympicsLogger.LogWarning($"Ignoring unparsable coin details: {coin}\nException:\n{exc.Message}\n{exc.StackTrace}");
                }
            AvailableCoins = coins;
            await RoomsManager.CheckJoinedRoomStatus(gameDataResponse);
        }

        private async UniTask GetElympicsUserData()
        {
            var logger = _logger.WithMethodName();
            logger.LogInfo("Start fetching user data...");
            var response = await WebSocketSession.SendRequest<ShowAuthResponseDto>(new ShowAuthDto());
            ElympicsUser = response.User.ToPublicModel();
            ElympicsLogger.State.SetNickname(ElympicsUser.Value.Nickname);
            logger.LogInfo("User data retrieved.");
        }

        private void SignOutInternal()
        {
            AuthData = null;
            ElympicsUser = null;
            if (WebSocketSession.IsConnected)
            {
                ElympicsLogger.LogInfo("Closing current websocket.");
                WebSocketSession.Disconnect(DisconnectionReason.ClientRequest);
                RoomsManager.Reset();
            }
            _logger.WithMethodName().LogInfo("User sign out.");
            ElympicsLogger.State.ClearUser();
            ElympicsLogger.State.SetRegion(null);
            ElympicsLogger.State.ClearRoom();
        }

        private void OnSuccessfullyConnected(bool reconnected) => ElympicsConnectionEstablished?.Invoke(new ElympicsConnectionData
        {
            AuthData = AuthData!,
            AutoReconnected = reconnected,
        });

        #endregion

        #region Rooms (IMatchLauncher)

        async UniTask IMatchLauncher.StartMatchmaking(IRoom room)
        {
            switch (_connectionState)
            {
                case ElympicsState.Connected:
                    ThrowIfLaunched(nameof(IMatchLauncher.StartMatchmaking));
                    await room.StartMatchmakingInternal();
                    SetConnectionState(ElympicsState.Matchmaking);
                    break;
                case ElympicsState.Disconnected:
                    _logger.WithMethodName().LogError(ErrorMessage(nameof(IMatchLauncher.StartMatchmaking)));
                    break;
                case ElympicsState.Connecting:
                case ElympicsState.Matchmaking:
                case ElympicsState.PlayingMatch:
                case ElympicsState.WatchReplay:
                case ElympicsState.Reconnecting:
                default:
                    throw new ElympicsException(ErrorMessage(nameof(IMatchLauncher.StartMatchmaking)));
            }
        }

        async UniTask IMatchLauncher.CancelMatchmaking(IRoom room, CancellationToken ct)
        {
            if (_connectionState != ElympicsState.Matchmaking)
            {
                _logger.WithMethodName().LogWarning(WarningMessage(nameof(IMatchLauncher.CancelMatchmaking)));
                return;
            }
            if (_matchmakingCancelRequested)
                throw new ElympicsException("Matchmaking cancellation already requested");
            try
            {
                _matchmakingCancelRequested = true;
                await room.CancelMatchmakingInternal(ct);
                SetConnectionState(ElympicsState.Connected);
            }
            catch (LobbyOperationException e) when (e.Kind == ErrorKind.RoomAlreadyInMatchedState)
            { }
            catch (LobbyOperationException)
            {
                SetConnectionState(ElympicsState.Connected);
                throw;
            }
            finally
            {
                _matchmakingCancelRequested = false;
            }
        }

        void IMatchLauncher.MatchmakingCompleted()
        {
            switch (_connectionState)
            {
                case ElympicsState.Matchmaking:
                    SetConnectionState(ElympicsState.Connected);
                    break;
                case ElympicsState.Reconnecting:
                    throw new ElympicsException(ErrorMessage(nameof(IMatchLauncher.MatchmakingCompleted)));
                case ElympicsState.Disconnected:
                case ElympicsState.Connecting:
                case ElympicsState.Connected:
                case ElympicsState.PlayingMatch:
                case ElympicsState.WatchReplay:
                default:
                    break;
            }
        }

        void IMatchLauncher.PlayMatch(RoomMatchData matchData)
        {
            if (_connectionState != ElympicsState.Connected || _launch != LaunchKind.None)
                throw new ElympicsException(ErrorMessage(nameof(IMatchLauncher.PlayMatch)));
            if (_launcher.State != MatchLauncherState.Idle)
                throw new InvalidOperationException($"The match launcher is {_launcher.State}, so the match found in the room cannot be played.");
            _logger.WithMethodName().LogInfo("Play match.");
            var details = matchData.MatchDetails;
            var match = new FoundMatch(matchData.MatchId, details.MatchedPlayersId, details.TcpUdpServerAddress, details.WebServerAddress, details.UserSecret,
                details.GameEngineData, details.MatchmakerData, matchData.QueueName, matchData.RegionName);
            BeginLaunch(LaunchKind.Match, ct => MatchLaunching.LaunchAsync(_launcher, _gameConfig.GameId, _useWeb, match, AuthData!.UserId, ct));
        }

        #endregion

        #region Replay

        public void WatchReplay(SnapshotAnalysisRetriever retriever)
        {
            if (_launch != LaunchKind.None || _connectionState is ElympicsState.Connecting or ElympicsState.Reconnecting or ElympicsState.Matchmaking)
                throw new ElympicsException(ErrorMessage(nameof(WatchReplay)));
            BeginLaunch(LaunchKind.Replay, ct => _launchReplay(_gameConfig.GameId, retriever, ct));
        }

        #endregion

        #region Launches

        private void BeginLaunch(LaunchKind kind, Func<CancellationToken, UniTask<ElympicsLaunchResult>> launch)
        {
            _launch = kind;
            UpdateReportedState();
            RunLaunch(launch).Forget();
        }

        private async UniTaskVoid RunLaunch(Func<CancellationToken, UniTask<ElympicsLaunchResult>> launch)
        {
            var logger = _logger.WithMethodName();
            ElympicsLaunchResult result;
            try
            {
                result = await launch(CancellationToken.None);
            }
            catch (Exception e)
            {
                logger.LogException(e);
                EndLaunch();
                return;
            }
            if (result.Success)
                return;
            logger.LogError(result.ToString());
            // A failed launch that was accepted ends at Idle; a rejected one never left it (PlayMatch only launches from Idle).
            if (_launcher.State == MatchLauncherState.Idle)
                EndLaunch();
        }

        private void OnLauncherStateChanged(MatchLauncherState state)
        {
            if (state == MatchLauncherState.Idle)
            {
                LaunchedMatch.Clear();
                EndLaunch();
            }
        }

        private void EndLaunch()
        {
            if (_launch == LaunchKind.None)
                return;
            _launch = LaunchKind.None;
            if (_connectionState == ElympicsState.Connected && !WebSocketSession.IsConnected)
                _connectionState = ElympicsState.Disconnected;
            UpdateReportedState();
        }

        private void ThrowIfLaunched(string action)
        {
            if (_launch != LaunchKind.None)
                throw new ElympicsException(ErrorMessage(action));
        }

        #endregion

        #region ICoinSource

        async UniTask<RollingsResponseDto?> ICoinSource.GetRollTournamentsFeeInternal(TournamentFeeRequestInfo[] requestData, CancellationToken ct) =>
            await GetRollTournamentsFeeInternal(requestData, ct);

        public async UniTask<TournamentFeeInfo?> GetRollTournamentsFee(TournamentFeeRequestInfo[] requestData, CancellationToken ct = default)
        {
            if (requestData.Length == 0)
                return null;

            var response = await GetRollTournamentsFeeInternal(requestData, ct);
            if (response == null)
                return null;

            var fees = new FeeInfo[response.Rollings.Count];
            for (var i = 0; i < fees.Length; i++)
            {
                var fee = response.Rollings[i];
                var coinId = requestData[i].CoinInfo.Id;
                var decimals = AvailableCoins?.FirstOrDefault(coin => coin.Id == coinId) is { } coinInfo && coinInfo.Id == coinId
                    ? coinInfo.Currency.Decimals
                    : throw new ElympicsException($"Coin with ID {coinId} was not found when processing rolling tournament fees.");
                fees[i] = new FeeInfo
                {
                    EntryFee = RawCoinConverter.FromRaw(fee.EntryFee, decimals),
                    Error = fee.Error,
                    EntryFeeRaw = fee.EntryFee,
                };
                RollingTournamentBetConfigIDs.AddOrUpdate(coinId, requestData[i].Prize, requestData[i].PlayersCount, fee.RollingTournamentBetConfigId);
            }

            return new TournamentFeeInfo { Fees = fees };
        }

        private async UniTask<RollingsResponseDto?> GetRollTournamentsFeeInternal(TournamentFeeRequestInfo[] requestData, CancellationToken ct)
        {
            var request = new RequestRollingsDto(
                GameId: Guid.Parse(_gameConfig.GameId),
                VersionId: _gameConfig.GameVersion,
                Rollings: requestData.Select(x => new RollingRequestDto(
                        CoinId: x.CoinInfo.Id,
                        Prize: RawCoinConverter.ToRaw(x.Prize, decimals: x.CoinInfo.Currency.Decimals),
                        PlayersCount: (uint)x.PlayersCount,
                        PrizeDistribution: x.PrizeDistribution ?? Array.Empty<decimal>()))
                    .ToList());

            return await WebSocketSession.SendRequest<RollingsResponseDto>(request, ct);
        }

        #endregion

        #region State

        private void SetConnectionState(ElympicsState state)
        {
            _connectionState = state;
            UpdateReportedState();
        }

        private void UpdateReportedState()
        {
            var newState = _launch switch
            {
                LaunchKind.Match => ElympicsState.PlayingMatch,
                LaunchKind.Replay => ElympicsState.WatchReplay,
                _ => _connectionState,
            };
            if (newState == State)
                return;
            var previousState = State;
            State = newState;
            ElympicsLogger.LogInfo($"Switch state from {previousState} to {newState}");
            CrossAssemblyEventBroadcaster.RaiseEvent(new ElympicsStateChanged
            {
                PreviousState = previousState,
                NewState = newState,
            });
        }

        private string ErrorMessage(string action) => $"Must not {action} during {State} state";
        private string WarningMessage(string action) => $"{action} during {State} state";

        #endregion

        public void Dispose()
        {
            _launcher.StateChanged -= OnLauncherStateChanged;
            WebSocketSession.Dispose();
        }
    }
}
#endif
