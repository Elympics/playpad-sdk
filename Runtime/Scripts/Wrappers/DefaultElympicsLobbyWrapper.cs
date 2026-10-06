#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.Models.Authentication;
using Elympics.Resolvers;
using Elympics.SnapshotAnalysis.Retrievers;
using ElympicsPlayPad.Auth;
using ElympicsPlayPad.Auth.Models;
using ElympicsPlayPad.ExternalCommunicators;
using ElympicsPlayPad.Lobby;
using ElympicsPlayPad.Rooms;
using MessagePack;
using MessagePack.Resolvers;
using Plugins.Elympics.Plugins.ParrelSync;
using UnityEngine;
using LobbyRoutes = ElympicsPlayPad.Lobby.InternalModels.Routes;

namespace ElympicsPlayPad.Wrappers
{
    public class DefaultElympicsLobbyWrapper : MonoBehaviour, IElympicsLobbyWrapper
    {
        [Tooltip("Default value of " + nameof(ShouldLoadGameplaySceneAfterMatchmaking) + ". It can be changed at runtime.")]
        [SerializeField] private bool shouldLoadGameplaySceneAfterMatchmaking = true;

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
                (gameId, retriever, ct) => ElympicsMatchLauncher.Current!.LaunchSnapshotReplayAsync(gameId, retriever, null, ct),
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

        /// <summary>Ordered like core's <see cref="ResolverRegistrator"/>.</summary>
        internal static MessagePackSerializerOptions CreateSerializerOptions() =>
            MessagePackSerializerOptions.Standard.WithResolver(CompositeResolver.Create(
                GeneratedResolver.Instance,
                AttributeFormatterResolver.Instance,
                BuiltinResolver.Instance,
                PrimitiveObjectResolver.Instance,
                MissingTypesResolver.Instance));

        public UniTask ConnectToElympicsAsync(ConnectionData data) => Client.ConnectToElympicsAsync(data);

        public UniTask ConnectStandaloneEditorToElympicsAsync(AuthData data, string region) =>
            Client.ConnectToElympicsAsync(new ConnectionData
            {
                AuthType = AuthType.ClientSecret,
                Region = new RegionData(region),
            });

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
            ElympicsClonesManager.IsClone() ? $"{ClientSecretPlayerPrefsKeyBase}_clone_{ElympicsClonesManager.GetCloneNumber()}" : ClientSecretPlayerPrefsKeyBase;

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
    }
}
