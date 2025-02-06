using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Elympics;
using ElympicsPlayPad.ExternalCommunicators.Tournament.Utility;
using UnityEngine;
using UnityEngine.Assertions;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    public class GenericSoloServerHandler : ElympicsMonoBehaviour, IServerHandlerGuid, IUpdatable
    {
        private const int PlayersNotConnectedToTheServerCheckInterval = 5;

        private TimeSpan _startGameTimeout;

        private int _playersNumber;
        private readonly HashSet<ElympicsPlayer> _playersConnected = new();

        private DateTime _waitToStartFinishTime;
        private bool _gameStarted;
        private bool _gameJustStarted;

        private SoloMatchEnder _soloMatchEnder;
        private SynchronizedRandomizerBase _synchronizedRandomizer;

        private bool _playerJustRejoined = false;
        private MatchesConfig _matchesConfig;
        private float _rejoiningTimeoutInSeconds;

        public event Action GameStarted;
        // Following events are meant for freezing game logic when waiting for the player to rejoin (if such feature is enabled)
        public event Action PlayerRejoined_ClientCallIncluded;
        public event Action PlayerDisconnected_ServerOnly;

        private void Awake()
        {
            _soloMatchEnder = FindObjectOfType<SoloMatchEnder>();
            Assert.IsNotNull(_soloMatchEnder);

            _synchronizedRandomizer = FindObjectOfType<SynchronizedRandomizerBase>();
            Assert.IsNotNull(_synchronizedRandomizer);

            _matchesConfig = MatchesConfig.LoadFromResources();
            Assert.IsNotNull(_matchesConfig);
            _rejoiningTimeoutInSeconds = _matchesConfig.RejoiningTimeoutInSeconds;
        }

        public void OnServerInit(InitialMatchPlayerDatasGuid initialMatchPlayerDatas)
        {
            var seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue); // random seed for no tournament, editor testing
            if (initialMatchPlayerDatas.CustomMatchmakingData != null && initialMatchPlayerDatas.CustomMatchmakingData.TryGetValue(TournamentConst.TournamentIdKey, out var tournamentId))
            {
                // existing tournament, online play
                seed = tournamentId.GetHashCode();
            }

            _synchronizedRandomizer.InitializeRandomization(seed);

            var timeoutSec = ElympicsConfig.LoadCurrentElympicsGameConfig().ConnectionConfig.sessionConnectTimeout;
            _startGameTimeout = TimeSpan.FromSeconds(timeoutSec);
            _playersNumber = initialMatchPlayerDatas.Count;
            var humansPlayers = initialMatchPlayerDatas.Count(x => !x.IsBot);
            Debug.Log($"[GenericServerHandler] - Game initialized for {initialMatchPlayerDatas.Count} players "
                               + $"(including {initialMatchPlayerDatas.Count - humansPlayers} bots).");
            Debug.Log($"[GenericServerHandler] - Waiting for {humansPlayers} human players to connect.");
            var sb = new StringBuilder()
                .AppendLine($"MatchId: {initialMatchPlayerDatas.MatchId}")
                .AppendLine($"QueueName: {initialMatchPlayerDatas.QueueName}")
                .AppendLine($"RegionName: :{initialMatchPlayerDatas.RegionName}")
                .AppendLine($"CustomMatchmakingData: {initialMatchPlayerDatas.CustomMatchmakingData?.Count.ToString() ?? "null"}")
                .AppendLine($"CustomRoomData: {(initialMatchPlayerDatas.CustomRoomData != null ? string.Join(", ", initialMatchPlayerDatas.CustomRoomData.Select(x => x.Key)) : "null")}")
                .AppendLine($"ExternalGameData: {initialMatchPlayerDatas.ExternalGameData?.Length.ToString() ?? "null"}");

            foreach (var playerData in initialMatchPlayerDatas)
                _ = sb.AppendLine($"[GenericServerHandler] - Player {playerData.UserId} {(playerData.IsBot ? "Bot" : "Human")} room {playerData.RoomId} teamIndex {playerData.TeamIndex}");
            Debug.Log(sb.ToString());

            _ = StartCoroutine(WaitForGameStartOrEnd());
        }

        private IEnumerator WaitForGameStartOrEnd()
        {
            _waitToStartFinishTime = DateTime.Now + _startGameTimeout;

            while (DateTime.Now < _waitToStartFinishTime)
            {
                if (_gameStarted)
                    yield break;

                Debug.Log("[GenericServerHandler] - Not all players connected yet...");
                yield return new WaitForSeconds(PlayersNotConnectedToTheServerCheckInterval);
            }

            Debug.LogWarning("[GenericServerHandler] - Forcing game server to quit because some players did not connect on time.\n"
                                      + "Connected players: "
                                      + string.Join(", ", _playersConnected));
            Elympics.EndGame();
        }

        public void OnPlayerDisconnected(ElympicsPlayer player)
        {
            Debug.Log($"[GenericServerHandler] - Player {player} disconnected.");

            _ = _playersConnected.Remove(player);

            PlayerDisconnected_ServerOnly?.Invoke();

            if (!_matchesConfig.RejoiningEnabled)
            {
                Debug.LogWarning("[GenericServerHandler] - Forcing game server to quit because one of the players has disconnected. Current score saved as the results");
                _soloMatchEnder.EndGame();
            }
        }

        public void OnPlayerConnected(ElympicsPlayer player)
        {
            Debug.Log($"[GenericServerHandler] - Player {player} connected.");

            _ = _playersConnected.Add(player);

            if (_gameStarted)
            {
                _playerJustRejoined = true;
            }
            else if (_playersConnected.Count == _playersNumber)
            {
                // Will be called only on the server
                _gameJustStarted = true;
                _gameStarted = true;
                Debug.Log("[GenericServerHandler] - All players have connected.");
            }
        }

        public void ElympicsUpdate()
        {
            // happens only on the server instance
            if (!Elympics.IsServer)
                return;

            if (_gameJustStarted)
            {
                _gameJustStarted = false;
                StartGameplay();
                StartGameplayAtClient(_synchronizedRandomizer.InitialSeed);
            }

            if (_playerJustRejoined)
            {
                _playerJustRejoined = false;
                HandleRejoin();
                HandleRejoinAtClient(_synchronizedRandomizer.InitialSeed);
            }

            if (_playersConnected.Count < _playersNumber && _gameStarted && _matchesConfig.RejoiningEnabled)
            {
                _rejoiningTimeoutInSeconds -= Elympics.TickDuration;

                if (_rejoiningTimeoutInSeconds <= 0)
                {
                    Debug.LogWarning($"[GenericServerHandler] - Rejoining timeout {_matchesConfig.RejoiningTimeoutInSeconds}s reached. Forcing game server to quit because one of the players has disconnected. Current score saved as the results");
                    _soloMatchEnder.EndGame();
                }
            }
        }

        private void HandleRejoin()
        {
            Debug.Log("[GenericServerHandler] - HandleRejoin");
            PlayerRejoined_ClientCallIncluded?.Invoke();
        }

        [ElympicsRpc(ElympicsRpcDirection.ServerToPlayers)]
        private void HandleRejoinAtClient(int initialSeed)
        {
            _synchronizedRandomizer.InitializeRandomization(initialSeed);
            HandleRejoin();
        }

        private void StartGameplay()
        {
            Debug.Log("[GenericServerHandler] - StartGameplay");
            GameStarted?.Invoke();
        }

        [ElympicsRpc(ElympicsRpcDirection.ServerToPlayers)]
        private void StartGameplayAtClient(int initialSeed)
        {
            _synchronizedRandomizer.InitializeRandomization(initialSeed);
            StartGameplay();
        }
    }
}
