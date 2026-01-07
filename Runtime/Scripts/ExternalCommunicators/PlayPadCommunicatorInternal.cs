#nullable enable
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.ElympicsSystems.Internal;
using Elympics.Models.Authentication;
using Elympics.Models.Matchmaking;
using Elympics.SnapshotAnalysis.Retrievers;
using ElympicsPlayPad.ExternalCommunicators.Replay;
using ElympicsPlayPad.Session;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ElympicsPlayPad
{
    internal class PlayPadCommunicatorInternal : ILobby, ISessionManagerAuthProvider
    {
        public AuthData? AuthData { get; private set; }
        public SnapshotAnalysisRetriever? SnapshotAnalysisRetriever => _externalReplayCommunicator?.SnapshotRetriever();
        public JoinedMatchMode MatchMode { get; private set; }
        public MatchmakingFinishedData? MatchDataGuid { get; private set; }
        private readonly IExternalReplayCommunicator? _externalReplayCommunicator;
        private readonly ElympicsGameConfig _gameConfig;
        public PlayPadCommunicatorInternal(IExternalReplayCommunicator? externalReplayCommunicator, ElympicsGameConfig gameConfig)
        {
            _externalReplayCommunicator = externalReplayCommunicator;
            _gameConfig = gameConfig;
        }

        public void PlayMatchInternal(MatchmakingFinishedData matchData)
        {
            MatchDataGuid = matchData;

            var isSinglePlayer = string.IsNullOrEmpty(matchData.WebServerAddress) && string.IsNullOrEmpty(matchData.TcpUdpServerAddress);

            SetUpMatch(isSinglePlayer ? JoinedMatchMode.SinglePlayer : JoinedMatchMode.Online);
            LoadGameplayScene();
        }
        private void LoadGameplayScene() => SceneManager.LoadScene(_gameConfig.GameplayScene);

        private void SetUpMatch(JoinedMatchMode mode) => MatchMode = mode;
        public UniTask Authenticate(AuthData cachedData, string region, bool autoRetry)
        {
            Debug.Log("Authenticated with cached data.");
            AuthData = cachedData;
            return UniTask.CompletedTask;
        }
        public void SignOut() => AuthData = null;
        public bool IsAuthenticated => AuthData != null;
    }
}
