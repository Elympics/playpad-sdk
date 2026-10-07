using Cysharp.Threading.Tasks;
using Elympics;
using ElympicsPlayPad.ExternalCommunicators;
using ElympicsPlayPad.ExternalCommunicators.Lobby.Models;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    public class EndGameView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI currentScoreText, tournamentBestScoreText, allTimeBestScoreText;
        [SerializeField] private Image tournamentBestScoreRibbon, allTimeBestScoreRibbon;

        [Header("Score Formats")]
        [SerializeField] private string currentScoreStringFormat = "Score: {0}";
        [SerializeField] private string tournamentBestScoreStringFormat = "Tournament Best Score: {0}";
        [SerializeField] private string allTimeBestScoreStringFormat = "All Time Best Score: {0}";

        private int shownPoints;

        public void Show(int points)
        {
            shownPoints = points;

            // Display the current score
            currentScoreText.text = string.Format(currentScoreStringFormat, points);

            // Please note that the "Check" methods are updating Tournament & All Time High Scores. These scores will automatically be updated after server closure, but at this point server is still open.
            UpdateHighScoreRibbonAndText(ElympicsBestScoreManager.CheckIsNewTournamentHighScore(points), ElympicsBestScoreManager.TournamentHighScore, tournamentBestScoreRibbon, tournamentBestScoreText, tournamentBestScoreStringFormat);
            UpdateHighScoreRibbonAndText(ElympicsBestScoreManager.CheckIsNewAllTimeHighScore(points), ElympicsBestScoreManager.AllTimeHighScore, allTimeBestScoreRibbon, allTimeBestScoreText, allTimeBestScoreStringFormat);

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Reusable method to update the best scores ribbons 
        /// </summary>
        private void UpdateHighScoreRibbonAndText(bool ribbonActive, int score, Image ribbon, TextMeshProUGUI text, string scoreFormat)
        {
            ribbon.gameObject.SetActive(ribbonActive);
            text.text = string.Format(scoreFormat, score);
        }


        /// <summary>
        /// Ends the match: the match launcher returns to the boot scene, which is the lobby scene.
        /// A web page that matchmakes gets the score first, and it may tear the game down on that.
        /// </summary>
        [UsedImplicitly] // by BackToLobbyButton
        public void ReturnToLobby()
        {
            if (TournamentPlayButton.HostMatchmakes)
                PlayPadCommunicator.Instance!.LobbyCommunicator!.Quit(new QuitArgs { Score = shownPoints });
            ElympicsMatchLauncher.Instance.ReturnToBootAsync().Forget();
        }
    }
}
