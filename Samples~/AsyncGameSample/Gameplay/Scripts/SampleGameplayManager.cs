using Elympics;
using System.Collections.Generic;
using UnityEngine;
using JetBrains.Annotations;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    public class SampleGameplayManager : ElympicsMonoBehaviour, IInitializable, IUpdatable, IClientHandlerGuid
    {
        [SerializeField] private int secondsToEndGameAutomatically = 30;
        [SerializeField] private ViewManager viewManager;

        private readonly ElympicsInt _remainingSecondsToEndGame = new ElympicsInt();
        private readonly ElympicsInt _points = new ElympicsInt();

        private bool pointsBumpRequested = false;
        private bool endGameRequested = false;

        private int CurrentGameTimeInSeconds => Mathf.FloorToInt(Elympics.Tick * Elympics.TickDuration);

        public void Initialize()
        {
            _remainingSecondsToEndGame.ValueChanged += (_, newValue) => viewManager.UpdateTimer(newValue);
            _points.ValueChanged += (_, newValue) => viewManager.UpdatePoints(newValue);

            _remainingSecondsToEndGame.Value = secondsToEndGameAutomatically;
            _points.Value = 0;
        }

        public void ElympicsUpdate()
        {
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
                pointsBumpRequested = false;
                _points.Value++; // needed to predict the outcome at the client and avoid reconciliations
                RpcBumpPoints();
            }

            if (endGameRequested)
            {
                endGameRequested = false;
                RpcEndGame();
            }
        }


        // Runs only at the Client
        public void OnMatchEnded(System.Guid _) => viewManager.ShowGameEndedView();

        [UsedImplicitly] // by EndGameButton
        public void RequestGameEnd() => endGameRequested = true;

        [ElympicsRpc(ElympicsRpcDirection.PlayerToServer)]
        private void RpcEndGame() => EndGameServer();

        private void EndGameServer()
        {
            if (!Elympics.IsServer)
                return;

            Elympics.EndGame(
                new ResultMatchPlayerDatas(
                    new List<ResultMatchPlayerData>
                    {
                        new ResultMatchPlayerData { MatchmakerData = new float[1] { _points.Value } }
                    }
                )
            );
        }


        // Please note it is only possible outside of ElympicsUpdate because Prediction in the ElympicsGameConfig was set to False
        [UsedImplicitly] // by BumpPointsButton
        public void RequestBumpPoints() => pointsBumpRequested = true;

        // Do not let players influencne their points directly like this in your game!
        // This implementation is only for testing and learning about Elympics SDK
        // But generally it would make cheating quite easy, so please ensure server authority and indirect points evaluation in your scoring system
        [ElympicsRpc(ElympicsRpcDirection.PlayerToServer)]
        private void RpcBumpPoints() => _points.Value++;
    }
}
