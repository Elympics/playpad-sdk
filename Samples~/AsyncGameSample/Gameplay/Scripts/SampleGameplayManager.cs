using Elympics;
using UnityEngine;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    // Its ElympicsBehaviour is predictable for World, so clients don't predict it:
    // the game state (e.g. points) changes only on the server, in RPCs.
    public class SampleGameplayManager : ElympicsMonoBehaviour, IInitializable, IUpdatable, IClientHandlerGuid
    {
        [SerializeField] private int secondsToEndGameAutomatically = 30;
        [SerializeField] private ViewManager viewManager;
        [SerializeField] private GameObject readyButton;

        private readonly ElympicsInt _tickOfGameStarted = new ElympicsInt();
        private readonly ElympicsInt _remainingSecondsToEndGame = new ElympicsInt();
        private readonly ElympicsInt _points = new ElympicsInt();

        private bool gameStarted = false;

        private int CurrentGameTimeInSeconds => Mathf.FloorToInt((Elympics.Tick - _tickOfGameStarted.Value) * Elympics.TickDuration);

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

            // Sends the Elympics score as the result.
            if (_remainingSecondsToEndGame.Value == 0)
                Elympics.EndGame();
        }

        // Runs only at the Client
        public void OnMatchEnded(System.Guid _) => viewManager.ShowGameEndedView(_points.Value);

        // The game starts once every player has pressed Ready.
        [UsedImplicitly] // by ReadyButton
        public void RequestReady()
        {
            Elympics.MarkReady();
            readyButton.SetActive(false);
        }

        [UsedImplicitly] // by EndGameButton
        [ElympicsRpc(ElympicsRpcDirection.PlayerToServer)]
        public void RequestGameEnd() => Elympics.EndGame();

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
            // The SDK sends it as the result, also when the match ends because the player left.
            Elympics.Score.SetScore(0, _points.Value);
            viewManager.RandomizeBackgroundColor();
        }
    }
}
