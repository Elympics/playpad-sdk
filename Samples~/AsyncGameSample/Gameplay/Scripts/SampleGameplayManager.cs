using Elympics;
using UnityEngine;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    public class SampleGameplayManager : SoloScoreProviderBase, IInitializable, IUpdatable, IClientHandlerGuid
    {
        [SerializeField] private int secondsToEndGameAutomatically = 30;
        [SerializeField] private ViewManager viewManager;

        private readonly ElympicsInt _tickOfGameStarted = new ElympicsInt();
        private readonly ElympicsInt _remainingSecondsToEndGame = new ElympicsInt();
        private readonly ElympicsInt _points = new ElympicsInt();

        private bool gameStarted = false;

        private bool pointsBumpRequested = false;
        private bool endGameRequested = false;

        private int CurrentGameTimeInSeconds => Mathf.FloorToInt((Elympics.Tick - _tickOfGameStarted.Value) * Elympics.TickDuration);

        public override float Score => _points.Value;

        public void Initialize()
        {
            _remainingSecondsToEndGame.ValueChanged += (_, newValue) => viewManager.UpdateTimer(newValue);
            _remainingSecondsToEndGame.ValueChanged += (_, newValue) =>
                {
                    if (_remainingSecondsToEndGame.Value % 5 == 0 && newValue != secondsToEndGameAutomatically && gameStarted)
                        viewManager.RandomizeBackgroundColor(true);
                };

            _points.ValueChanged += (_, newValue) => viewManager.UpdatePoints(newValue);

            _remainingSecondsToEndGame.Value = secondsToEndGameAutomatically;
            _points.Value = 0;

            var serverHandler = FindObjectOfType<GenericSoloServerHandler>();
            Assert.IsNotNull(serverHandler);
            serverHandler.GameStarted += StartGame;
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

            if (!Elympics.IsClient)
                return;

            if (pointsBumpRequested)
            {
                Debug.Log("BumpPoints at client");
                pointsBumpRequested = false;
                _points.Value++; // needed to predict the outcome at the client and avoid reconciliations
                viewManager.RandomizeBackgroundColor();

                RpcBumpPoints();
            }

            if (endGameRequested)
            {
                endGameRequested = false;
                RpcEndGame();
            }
        }

        // Runs only at the Client
        public void OnMatchEnded(System.Guid _) => viewManager.ShowGameEndedView(_points.Value);

        [UsedImplicitly] // by EndGameButton
        public void RequestGameEnd() => endGameRequested = true;

        [ElympicsRpc(ElympicsRpcDirection.PlayerToServer)]
        private void RpcEndGame() => EndGameServer();

        private void EndGameServer()
        {
            var soloMatchEnder = FindObjectOfType<SoloMatchEnder>();
            Assert.IsNotNull(soloMatchEnder);

            soloMatchEnder.EndGame();
        }


        // Please note it is only possible outside of ElympicsUpdate because Prediction in the ElympicsGameConfig was set to False
        [UsedImplicitly] // by BumpPointsButton
        public void RequestBumpPoints()
        {
            Debug.Log("RequestBumpPoints at client");
            pointsBumpRequested = true;
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
