using Elympics;
using System.Collections.Generic;
using UnityEngine;
using JetBrains.Annotations;
using System;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    public class SampleGameplayManager : ElympicsMonoBehaviour, IInitializable, IUpdatable
    {
        [SerializeField] private int secondsToEndGameAutomatically = 30;
        [SerializeField] private ViewManager viewManager;

        private readonly ElympicsInt remainingSecondsToEndGame = new ElympicsInt();
        private bool gameEndRequested = false;

        private Guid matchId;

        private int CurrentGameTimeInSeconds => Mathf.FloorToInt(Elympics.Tick * Elympics.TickDuration);

        private void Awake()
        {
            // Remembering matchId to display respect at the end
            var joinedRooms = ElympicsLobbyClient.Instance.RoomsManager.ListJoinedRooms();
            matchId = joinedRooms.Count > 0 ? (joinedRooms[0].State.MatchmakingData?.MatchData?.MatchId ?? Guid.Empty) : Guid.Empty;
        }

        public void Initialize()
        {
            remainingSecondsToEndGame.ValueChanged += (_, newValue) => viewManager.UpdateTimer(newValue);

            remainingSecondsToEndGame.Value = secondsToEndGameAutomatically;
        }

        public void ElympicsUpdate()
        {
            if (gameEndRequested && Elympics.IsClient)
            {
                EndGame(viewManager.Points); // only visuals, for client
                RpcEndGame(viewManager.Points); // executive, server side

                gameEndRequested = false;
                return;
            }

            remainingSecondsToEndGame.Value = Mathf.Max(0, secondsToEndGameAutomatically - CurrentGameTimeInSeconds);

            if (remainingSecondsToEndGame.Value == 0)
                EndGame(viewManager.Points);
        }

        [UsedImplicitly] // by EndGameButton
        public void RequestGameEnd() => gameEndRequested = true;

        [ElympicsRpc(ElympicsRpcDirection.PlayerToServer)]
        private void RpcEndGame(int points)
        {
            // Do not let players set their points like this in your game!
            // This implementation is only for testing and learning about Elympics SDK
            // But generally it would make cheating very easy, so please ensure server authority on your scoring system

            EndGame(points);
        }

        // TODO: separate to client's and server's for cleaner code
        private void EndGame(int points)
        {
            if (Elympics.IsServer)
                Elympics.EndGame(
                    new ResultMatchPlayerDatas(
                        new List<ResultMatchPlayerData>
                        {
                            new ResultMatchPlayerData { MatchmakerData = new float[1] { points } }
                        }));

            if (Elympics.IsClient)
                viewManager.ShowGameEndedView(matchId);
        }
    }
}
