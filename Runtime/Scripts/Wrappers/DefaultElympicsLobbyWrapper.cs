#nullable enable
#if UNITY_EDITOR || !UNITY_WEBGL || ELYMPICS_PLAYPAD_KEEP_LOBBY
using System;
using System.Collections.Generic;
using System.Threading;
using Elympics.Cysharp.Threading.Tasks;
using Elympics;
using Elympics.Models.Authentication;
using Elympics.SnapshotAnalysis.Retrievers;
using ElympicsPlayPad.Auth;
using ElympicsPlayPad.Auth.Models;
using ElympicsPlayPad.ExternalCommunicators;
using ElympicsPlayPad.Lobby;
using ElympicsPlayPad.Rooms;
using ElympicsPlayPad.Serialization.Resolvers;
using MessagePack;
using MessagePack.Resolvers;
using LobbyRoutes = ElympicsPlayPad.Lobby.InternalModels.Routes;
#endif
using ElympicsPlayPad.Utility;
using UnityEngine;

namespace ElympicsPlayPad.Wrappers
{
    /// <remarks>WebGL builds keep the lobby only with ELYMPICS_PLAYPAD_KEEP_LOBBY. Without it this component is empty.</remarks>
    [DefaultExecutionOrder(ElympicsLobbyExecutionOrders.LobbyWrapper)]
    public class DefaultElympicsLobbyWrapper : MonoBehaviour
#if UNITY_EDITOR || !UNITY_WEBGL || ELYMPICS_PLAYPAD_KEEP_LOBBY
        , IElympicsLobbyWrapper
#endif
    {
        [Tooltip("Default value of ShouldLoadGameplaySceneAfterMatchmaking. It can be changed at runtime.")]
        [SerializeField] private bool shouldLoadGameplaySceneAfterMatchmaking = true;
#if UNITY_EDITOR || !UNITY_WEBGL || ELYMPICS_PLAYPAD_KEEP_LOBBY

        private LobbyClient? _client;
        internal LobbyClient Client => _client ?? throw new InvalidOperationException($"{nameof(DefaultElympicsLobbyWrapper)} has not been initialized.");

        public event Action<ElympicsConnectionData>? ElympicsConnectionEstablished;
        public event Action<ElympicsConnectionLostData>? ElympicsConnectionLost;

        public ElympicsState State => Client.State;
        public IRoomsManager RoomsManager => Client.RoomsManager;
        public AuthData? AuthData => Client.AuthData;
        public ElympicsUser? ElympicsUser => Client.ElympicsUser;
        public bool IsAuthenticated => Client.IsAuthenticated;
        public IWebSocketSession WebSocketSession => Client.WebSocketSession;
        public string CurrentRegion => Client.CurrentRegion;
        public IReadOnlyCollection<string>? AvailableRegions => Client.AvailableRegions;
        public IReadOnlyCollection<CoinInfo>? AvailableCoins => Client.AvailableCoins;

        public bool ShouldLoadGameplaySceneAfterMatchmaking
        {
            get => Client.ShouldLoadGameplaySceneAfterMatchmaking;
            set => Client.ShouldLoadGameplaySceneAfterMatchmaking = value;
        }

        private void Awake()
        {
            // A duplicate communicator prefab is destroyed by PlayPadCommunicator.
            if (PlayPadCommunicator.Instance != null && PlayPadCommunicator.Instance.gameObject != gameObject)
                return;

            var config = ElympicsConfig.Load() ?? throw new InvalidOperationException($"No {nameof(ElympicsConfig)} instance found.");
            var gameConfig = config.GetCurrentGameConfig()
                ?? throw new InvalidOperationException($"No {nameof(ElympicsGameConfig)} instance found. Make sure {nameof(ElympicsConfig)} is set up correctly.");
            var parameters = ApplicationParameters.Parameters;
            var lobbyUrl = parameters.LobbyEndpoint.GetValue(config.GetV2Endpoint("lobby")).GetAbsoluteOrRelativeString().AppendPathSegments(LobbyRoutes.Base).ToString();
            var authUrl = parameters.AuthEndpoint.GetValue(config.GetV2Endpoint("auth")).GetAbsoluteOrRelativeString();
            var serializerOptions = CreateSerializerOptions();

            _client = new LobbyClient(
                new RemoteAuthClient(authUrl),
                GetOrCreateClientSecret(),
                new DefaultRegionRetriever(),
                ElympicsMatchLauncher.Instance,
                gameConfig,
                lobbyUrl,
                controller => new WebSocketSession(controller, MainThreadDispatcher.Instance, serializerOptions))
            {
                ShouldLoadGameplaySceneAfterMatchmaking = shouldLoadGameplaySceneAfterMatchmaking,
            };
            _client.ElympicsConnectionEstablished += data => ElympicsConnectionEstablished?.Invoke(data);
            _client.ElympicsConnectionLost += data => ElympicsConnectionLost?.Invoke(data);
        }

        private void OnDestroy() => _client?.Dispose();

        /// <summary>PlayPad's lobby formatters first, then core's resolvers registered by <see cref="ResolverRegistrator"/>.</summary>
        internal static MessagePackSerializerOptions CreateSerializerOptions() =>
            MessagePackSerializerOptions.Standard.WithResolver(CompositeResolver.Create(PlayPadGeneratedResolver.Instance, StaticCompositeResolver.Instance));

        public UniTask ConnectToElympicsAsync(ConnectionData data) => Client.ConnectToElympicsAsync(data);

        public UniTask Authenticate(AuthData authData, string region, bool autoRetry) =>
            Client.ConnectToElympicsAsync(new ConnectionData
            {
                Region = new RegionData(region),
                AuthFromCacheData = new CachedAuthData(authData, autoRetry),
            });

        public void SignOut() => Client.SignOut();

        public UniTask<TournamentFeeInfo?> GetRollTournamentsFee(TournamentFeeRequestInfo[] requestData, CancellationToken ct = default) =>
            Client.GetRollTournamentsFee(requestData, ct);

        void IElympicsLobbyWrapper.WatchReplay(SnapshotAnalysisRetriever retriever) => Client.WatchReplay(retriever);

        private const string ClientSecretPlayerPrefsKeyBase = "Elympics/AuthToken";

        private static string ClientSecretPlayerPrefsKey =>
            PlayModeInstance.PlayerName is { } playerName ? $"{ClientSecretPlayerPrefsKeyBase}_{playerName}" : ClientSecretPlayerPrefsKeyBase;

        private static string GetOrCreateClientSecret()
        {
            var parameterValue = ApplicationParameters.Parameters.ClientSecret.GetValue();
            if (!string.IsNullOrEmpty(parameterValue))
                return parameterValue!;

            if (parameterValue == "")
                return CreateNewClientSecret();

            var key = ClientSecretPlayerPrefsKey;
            if (!PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.SetString(key, CreateNewClientSecret());
                PlayerPrefs.Save();
            }
            return PlayerPrefs.GetString(key);
        }

        private static string CreateNewClientSecret() => Guid.NewGuid().ToString();
#endif
    }
}
