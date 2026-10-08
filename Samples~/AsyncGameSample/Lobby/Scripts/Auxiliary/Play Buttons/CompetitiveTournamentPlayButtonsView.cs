using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    public class CompetitiveTournamentPlayButtonsView : PlayButtonsViewBase
    {
        [SerializeField] private Button trainButton;

        public override void SubscribePlayButtonCallback(UnityAction onClickTournamentPlay, UnityAction onClickTrainingPlay)
        {
            base.SubscribePlayButtonCallback(onClickTournamentPlay, onClickTrainingPlay);

            trainButton.onClick.AddListener(onClickTrainingPlay);
        }

        protected override void RemoveCallbacks()
        {
            base.RemoveCallbacks();
            trainButton.onClick.RemoveAllListeners();
        }
    }
}
