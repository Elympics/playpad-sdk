using System;
using System.Collections.Generic;
using Elympics;
using UnityEngine;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    [RequireComponent(typeof(MatchEnder))]
    public class GenericServerHandler : ElympicsMonoBehaviour, IServerHandlerGuid, IMatchStartHandler, IInitializable, IUpdatable
    {
        [Tooltip("Ends the match with the " + nameof(MatchEnder) + " when any or all players leave after the match start. "
            + "Set terminateWhenPlayersLeave in the game config to None, so the SDK doesn't end the match with the Elympics score instead.")]
        [SerializeField] private TerminationOption autoTerminationOnLeft = TerminationOption.Any;
        [Tooltip("Viable only when " + nameof(autoTerminationOnLeft) + " is set to None.")]
        [SerializeField] private float rejoiningTimeoutInSeconds = 120f;

        private bool _gameJustStarted = false;
        private bool _anyPlayerJustRejoined = false;

        private readonly HashSet<ElympicsPlayer> _playersConnected = new();
        private MatchEnder _matchEnder;
        [SerializeField] private SynchronizedRandomizerBase synchronizedRandomizer;


        public event Action GameJustStarted;
        // Following events are meant for freezing game logic when waiting for the player to rejoin (if such a feature is enabled - with autoTerminationOnLeft set to None)
        public event Action PlayerRejoined_ClientCallIncluded;
        public event Action PlayerDisconnected_ServerOnly;


        private void Awake()
        {
            _matchEnder = GetComponent<MatchEnder>();

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

            if (Elympics.IsMatchStarted)
                _anyPlayerJustRejoined = true;
        }

        public void OnPlayerDisconnected(ElympicsPlayer player)
        {
            _ = _playersConnected.Remove(player);

            PlayerDisconnected_ServerOnly?.Invoke();

            if (!Elympics.IsMatchStarted)
                return;

            if (autoTerminationOnLeft == TerminationOption.Any
                || (autoTerminationOnLeft == TerminationOption.All && _playersConnected.Count == 0))
            {
                Debug.Log($"[GenericServerHandler] - Player {player} disconnected. Ending the match ({nameof(autoTerminationOnLeft)}: {autoTerminationOnLeft})");
                CloseMatch();
            }
        }

        public void OnMatchStarted(long startTick)
        {
            if (Elympics.IsServer)
                _gameJustStarted = true;
        }

        private void CloseMatch() => _matchEnder.EndMatch();

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

            if (_playersConnected.Count == 0 && Elympics.IsMatchStarted && autoTerminationOnLeft == TerminationOption.None)
            {
                rejoiningTimeoutInSeconds -= Elympics.TickDuration;

                if (rejoiningTimeoutInSeconds <= 0)
                {
                    Debug.LogWarning($"[GenericServerHandler] - Rejoining timeout {rejoiningTimeoutInSeconds}s reached. Forcing game server to quit because one of the players has disconnected. Current score saved as the results");
                    CloseMatch();
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
