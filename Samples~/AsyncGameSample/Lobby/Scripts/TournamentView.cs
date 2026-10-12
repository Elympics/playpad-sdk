using UnityEngine;
using ElympicsPlayPad.ExternalCommunicators;
using TMPro;
using ElympicsPlayPad.ExternalCommunicators.Ui;
using JetBrains.Annotations;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    public class TournamentView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI tournamentTitle;

        [Header("Timer Text References")]
        [SerializeField] private TextMeshProUGUI tournamentTimerLabel;
        [SerializeField] private TextMeshProUGUI tournamentTimer;

        [Header("Prize Pool References")]
        [SerializeField] private TextMeshProUGUI prizePoolValue;
        [SerializeField] private Image prizePoolImage;

        private TournamentTimer _tournamentTimerLogic;

        private IExternalUiCommunicator UiCommunicator => PlayPadCommunicator.Instance?.ExternalUiCommunicator;

        public static void OnStart()
        { }

        private void Update()
        {
            if (_tournamentTimerLogic == null)
                return;
            var (timer, label) = _tournamentTimerLogic.GetTimerAndLabel();
            tournamentTimerLabel.text = label;
            tournamentTimer.text = timer;
        }

        [UsedImplicitly]
        public void ShowSwitchTournamentPlayPadView() => UiCommunicator.DisplayTournamentsListing().Forget();

        [UsedImplicitly]
        public void ShowTournamentRewardsPlayPadView() => UiCommunicator.DisplayTournamentRewards().Forget();
    }
}
