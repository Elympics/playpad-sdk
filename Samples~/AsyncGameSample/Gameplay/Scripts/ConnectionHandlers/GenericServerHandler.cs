using System;
using System.Collections.Generic;
using Elympics;
using UnityEngine;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    // Ending the match when players leave is set by terminateWhenPlayersLeave in the game config.
    public class GenericServerHandler : ElympicsMonoBehaviour, IServerHandlerGuid, IGameStartHandler, IInitializable, IUpdatable
    {
        private bool _gameJustStarted = false;
        private bool _anyPlayerJustRejoined = false;

        private readonly HashSet<ElympicsPlayer> _playersConnected = new();
        [SerializeField] private SynchronizedRandomizerBase synchronizedRandomizer;


        public event Action GameJustStarted;
        // Following events are meant for freezing game logic when waiting for the player to rejoin
        // (if terminateWhenPlayersLeave in the game config lets the match continue)
        public event Action PlayerRejoined_ClientCallIncluded;
        public event Action PlayerDisconnected_ServerOnly;


        private void Awake()
        {
            if (synchronizedRandomizer == null)
                throw new NullReferenceException($"Make sure that your randomization system inherits from {nameof(SynchronizedRandomizerBase)} and is assigned to the {nameof(GenericServerHandler)} component");
        }

        public void Initialize()
        {
            // The match counts only once every player confirms that the game launched for them.
            if (Elympics.IsClient)
                Elympics.MarkReady();
        }

        public void OnServerInit(InitialMatchPlayerDatasGuid initialMatchPlayerDatas)
        {
            var seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue); // random seed for no tournament, editor testing
            if (initialMatchPlayerDatas.CustomMatchmakingData != null && initialMatchPlayerDatas.CustomMatchmakingData.TryGetValue(TournamentConst.TournamentIdKey, out var tournamentId))
            {
                // existing tournament, online play
                seed = tournamentId.GetHashCode();
            }

            synchronizedRandomizer.InitializeRandomization(seed);
        }

        public void OnPlayerConnected(ElympicsPlayer player)
        {
            _ = _playersConnected.Add(player);

            if (Elympics.IsGameStarted)
                _anyPlayerJustRejoined = true;
        }

        public void OnPlayerDisconnected(ElympicsPlayer player)
        {
            _ = _playersConnected.Remove(player);

            PlayerDisconnected_ServerOnly?.Invoke();
        }

        public void OnGameStarted(long startTick)
        {
            if (Elympics.IsServer)
                _gameJustStarted = true;
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
                StartGameplayAtClient(synchronizedRandomizer.InitialSeed);
            }

            if (_anyPlayerJustRejoined)
            {
                _anyPlayerJustRejoined = false;
                HandleRejoin();
                HandleRejoinAtClient(synchronizedRandomizer.InitialSeed);
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
            synchronizedRandomizer.InitializeRandomization(initialSeed);
            HandleRejoin();
        }

        private void StartGameplay()
        {
            Debug.Log("[GenericServerHandler] - StartGameplay");
            GameJustStarted?.Invoke();
        }

        [ElympicsRpc(ElympicsRpcDirection.ServerToPlayers)]
        private void StartGameplayAtClient(int initialSeed)
        {
            synchronizedRandomizer.InitializeRandomization(initialSeed);
            StartGameplay();
        }
    }
}
