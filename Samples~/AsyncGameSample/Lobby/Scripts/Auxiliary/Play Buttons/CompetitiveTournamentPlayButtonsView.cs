using UnityEngine;
using UnityEngine.UI;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    public class CompetitiveTournamentPlayButtonsView : PlayButtonsViewBase
    {
        [SerializeField] private Button trainButton;

        protected override void RemoveCallbacks()
        {
            base.RemoveCallbacks();
            trainButton.onClick.RemoveAllListeners();
        }
    }
}
