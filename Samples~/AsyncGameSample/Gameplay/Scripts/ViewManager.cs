using System;
using TMPro;
using UnityEngine;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    public class ViewManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timer;
        [SerializeField] private TextMeshProUGUI pointsDisplay;
        [SerializeField] private EndGameView gameEndedView;

        public void UpdateTimer(int remainingSeconds) => timer.text = remainingSeconds.ToString();
        public void UpdatePoints(int pointsValue) => pointsDisplay.text = pointsValue.ToString();

        public void ShowGameEndedView(int points)
        {
            gameEndedView.Show(points);
        }
    }
}
