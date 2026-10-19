using Elympics;
using UnityEngine;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    // Its ElympicsBehaviour is predictable for World, so clients don't predict it:
    // the game state (e.g. points) changes only on the server, in RPCs.
    public class SampleGameplayManager : ScoreProviderBase, IInitializable, IUpdatable, IClientHandlerGuid
    {
        [SerializeField] private int secondsToEndGameAutomatically = 30;
        [SerializeField] private ViewManager viewManager;

        private readonly ElympicsInt _tickOfGameStarted = new ElympicsInt();
        private readonly ElympicsInt _remainingSecondsToEndGame = new ElympicsInt();
        private readonly ElympicsInt _points = new ElympicsInt();

        private bool gameStarted = false;

        private int CurrentGameTimeInSeconds => Mathf.FloorToInt((Elympics.Tick - _tickOfGameStarted.Value) * Elympics.TickDuration);

        public override float[] Scores => new float[] { _points.Value };

        public void Initialize()
        {
            _remainingSecondsToEndGame.ValueChanged += (_, newValue) => viewManager.UpdateTimer(newValue);
            _remainingSecondsToEndGame.ValueChanged += (_, newValue) =>
                {
                    if (_remainingSecondsToEndGame.Value % 5 == 0 && newValue != secondsToEndGameAutomatically && gameStarted)
                        viewManager.RandomizeBackgroundColor();
                };

            _points.ValueChanged += (_, newValue) => viewManager.UpdatePoints(newValue);

            _remainingSecondsToEndGame.Value = secondsToEndGameAutomatically;
            _points.Value = 0;

            var serverHandler = FindAnyObjectByType<GenericServerHandler>();
            Assert.IsNotNull(serverHandler);
            serverHandler.GameJustStarted += StartGame;
        }

        private void StartGame()
        {
            gameStarted = true;

            _tickOfGameStarted.Value = (int)Elympics.Tick;
            viewManager.RandomizeBackgroundColor(true);
        }

        public void ElympicsUpdate()
        {
            if (!gameStarted)
                return;

            _remainingSecondsToEndGame.Value = Mathf.Max(0, secondsToEndGameAutomatically - CurrentGameTimeInSeconds);

            if (_remainingSecondsToEndGame.Value == 0)
            {
                EndGameServer();
                return;
            }
        }

        // Runs only at the Client
        public void OnMatchEnded(System.Guid _) => viewManager.ShowGameEndedView(_points.Value);

        [UsedImplicitly] // by EndGameButton
        public void RequestGameEnd() => RpcEndGame();

        [ElympicsRpc(ElympicsRpcDirection.PlayerToServer)]
        private void RpcEndGame() => EndGameServer();

        private void EndGameServer()
        {
            var matchEnder = FindAnyObjectByType<MatchEnder>();
            Assert.IsNotNull(matchEnder);

            matchEnder.EndMatch();
        }

        [UsedImplicitly] // by BumpPointsButton
        public void RequestBumpPoints()
        {
            Debug.Log("BumpPoints at client");

            viewManager.RandomizeBackgroundColor();

            RpcBumpPoints();
        }

        // Do not let players influencne their points directly like this in your game!
        // This implementation is only for testing and learning about Elympics SDK
        // But generally it would make cheating quite easy, so please ensure server authority and indirect points evaluation in your scoring system
        [ElympicsRpc(ElympicsRpcDirection.PlayerToServer)]
        private void RpcBumpPoints()
        {
            Debug.Log("BumpPoints at server");

            _points.Value++;
            viewManager.RandomizeBackgroundColor();
        }
    }
}
