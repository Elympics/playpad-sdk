using Elympics;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    public class SoloMatchEnder : ElympicsMonoBehaviour
    {
        private SoloScoreProviderBase scoreProvider;
        private MatchesConfig matchesConfig;

        private void Awake()
        {
            scoreProvider = FindObjectOfType<SoloScoreProviderBase>();
            Assert.IsNotNull(scoreProvider, $"Make sure that your score management inherits from {nameof(SoloScoreProviderBase)}");

            matchesConfig = MatchesConfig.LoadFromResources();
            Assert.IsNotNull(matchesConfig);
        }

        public void EndGame()
        {
            if (!Elympics.IsServer)
                return;

            float score = scoreProvider.Score;

            if (matchesConfig.ShouldZeroScoreBeInterpretedAsNotPlayedMatch && score == 0f)
                Elympics.EndGame();
            else
                Elympics.EndGame(
                    new ResultMatchPlayerDatas(
                        new List<ResultMatchPlayerData>
                        {
                            new ResultMatchPlayerData { MatchmakerData = new float[1] { score } }
                        }
                    )
                );
        }
    }
}
