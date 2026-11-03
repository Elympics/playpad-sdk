using System;
using Elympics;
using UnityEngine;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    // Ending the match when players leave is set by terminateWhenPlayersLeave in the game config.
    public class GenericServerHandler : ElympicsMonoBehaviour, IServerHandlerGuid, IUpdatable
    {
        private bool _gameplayStarted;
        private bool _anyPlayerJustRejoined;

        [SerializeField] private SynchronizedRandomizerBase synchronizedRandomizer;

        public event Action GameJustStarted;
        // For freezing the game logic while waiting for a player to rejoin.
        public event Action PlayerRejoined_ClientCallIncluded;
        public event Action PlayerDisconnected_ServerOnly;

        private void Awake()
        {
            if (synchronizedRandomizer == null)
                throw new NullReferenceException($"Make sure that your randomization system inherits from {nameof(SynchronizedRandomizerBase)} and is assigned to the {nameof(GenericServerHandler)} component");
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
            if (Elympics.IsGameStarted)
                _anyPlayerJustRejoined = true;
        }

        public void OnPlayerDisconnected(ElympicsPlayer player) => PlayerDisconnected_ServerOnly?.Invoke();

        public void ElympicsUpdate()
        {
            if (!Elympics.IsServer)
                return;

            if (Elympics.IsGameStarted && !_gameplayStarted)
            {
                _gameplayStarted = true;
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
