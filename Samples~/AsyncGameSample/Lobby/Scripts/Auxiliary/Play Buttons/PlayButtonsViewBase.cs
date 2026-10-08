using ElympicsPlayPad.ExternalCommunicators.GameStatus.Models;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    public abstract class PlayButtonsViewBase : MonoBehaviour
    {
        [SerializeField] protected Button playButton;
        [SerializeField] protected TextMeshProUGUI playButtonText;
        [SerializeField] protected Image playButtonImage;

        /// <summary>
        /// These sprites will be determined by the playpad PlayStatusInfo and should be changed.
        /// These are currently assigned in TournamentPlayButton via the inspector to avoid assigning the in each different play button view,
        /// but if you want different sprites for each play button please override the UpdatePlayButton() method.
        /// Then, add your own sprites to the new PlayButtonsViewBase child class.
        /// </summary>
        private Sprite _playAvailableSprite, _userActionRequiredSprite, _playBlockedSprite;

        /// <summary>
        /// OnDestroy MonoBehaviour will be called for all buttons, each button should override the RemoveCallbacks method to handle itself correctly OnDestroy
        /// </summary>
        private void OnDestroy() => RemoveCallbacks();

        /// <summary>
        /// Subscribe all buttons to the correct action. Example: OnClickTournamentPlay should be used by all PlayButtons, whereas OnClickTrainingPlay should only be used by Train buttons
        /// </summary>
        public virtual void SubscribePlayButtonCallback(UnityAction onClickTournamentPlay, UnityAction onClickTrainingPlay)
        {
            //Remove all listeners first
            RemoveCallbacks();

            playButton.onClick.AddListener(onClickTournamentPlay);
        }

        public void AssignPlayButtonSprites(Sprite playAvailableSprite, Sprite userActionRequiredSprite, Sprite playBlockedSprite)
        {
            _playAvailableSprite = playAvailableSprite;
            _userActionRequiredSprite = userActionRequiredSprite;
            _playBlockedSprite = playBlockedSprite;
        }

        public void UpdatePlayButton(PlayStatusInfo info)
        {
            playButtonText.text = info.LabelInfo;

            switch (info.PlayStatus)
            {
                case PlayStatus.Play:
                    playButtonImage.sprite = _playAvailableSprite;
                    playButton.interactable = true;
                    break;
                case PlayStatus.UserActionRequired:
                    playButtonImage.sprite = _userActionRequiredSprite;
                    playButton.interactable = true;
                    break;
                case PlayStatus.Blocked:
                    playButtonImage.sprite = _playBlockedSprite;
                    playButton.interactable = false;
                    break;
                default:
                    throw new Exception("Unsupported PlayStatus detected");
            }
        }

        /// <summary>
        /// This method should be overriden by the derived classes in order to correctly remove the callbacks from the appropriate buttons.
        /// </summary>
        protected virtual void RemoveCallbacks() => playButton.onClick.RemoveAllListeners();
    }
}
