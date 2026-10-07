using System;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.ExternalCommunicators;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Extensions;
using ElympicsPlayPad.ExternalCommunicators.GameStatus;
using ElympicsPlayPad.ExternalCommunicators.GameStatus.Models;
using ElympicsPlayPad.ExternalCommunicators.Lobby.Models;
using ElympicsPlayPad.ExternalCommunicators.Tournament;
using ElympicsPlayPad.Tournament.Data;
using UnityEngine;
using UnityEngine.Assertions;
#if UNITY_EDITOR || !UNITY_WEBGL || ELYMPICS_PLAYPAD_KEEP_LOBBY
using ElympicsPlayPad.Rooms;
#endif

namespace ElympicsPlayPad.Samples.AsyncGame
{
    public class TournamentPlayButton : MonoBehaviour
    {
        private const string ConnectingSubText = "Preparing the match...";

        [Header("Matchmaking")]
        [SerializeField] private string playQueue = "solo";
        [SerializeField] private ErrorPopup errorScreen;

        [Header("Play button")]
        [SerializeField] private Sprite playAvailableSprite;
        [SerializeField] private Sprite userActionRequiredSprite;
        [SerializeField] private Sprite playBlockedSprite;
        [SerializeField] private PlayButtonsViewBase[] playButtonViews;

        private PlayButtonsViewBase currentPlayButtonView;
        private bool hostMatchRequested;

        /// <summary>Without its own lobby (LaunchMode Gameplay), PlayPad gets the match from the web page, which also owns the Play button.</summary>
        public static bool HostMatchmakes => PlayPadCommunicator.Instance?.SessionManager.CurrentSession is { } session && !session.LaunchMode.HasLobby();

        private IExternalGameStatusCommunicator PlayStatusCommunicator => PlayPadCommunicator.Instance.GameStatusCommunicator;
        private IExternalTournamentCommunicator TournamentCommunicator => PlayPadCommunicator.Instance.TournamentCommunicator;

        private void Awake()
        {
            Assert.IsNotNull(errorScreen);
            Assert.IsNotNull(playAvailableSprite);
            Assert.IsNotNull(userActionRequiredSprite);
            Assert.IsNotNull(playBlockedSprite);
        }

        public void OnStart()
        {
            //Subscribe callbacks for play button & train button, each play button will handle it as needed
            for (var i = 0; i < playButtonViews.Length; i++)
            {
                playButtonViews[i].SubscribePlayButtonCallback(PlayTournament, PlayTraining);
                playButtonViews[i].AssignPlayButtonSprites(playAvailableSprite, userActionRequiredSprite, playBlockedSprite);
            }

            TournamentCommunicator.TournamentUpdated += UpdateTournamentView;
            UpdateTournamentView(TournamentCommunicator.CurrentTournament ?? default);

            if (HostMatchmakes)
            {
                PlayHostMatchWhenReady();
                return;
            }

            if (PlaysWithoutTournament)
            {
                UpdatePlayButton(new PlayStatusInfo { PlayStatus = PlayStatus.Play, LabelInfo = "Play" });
                return;
            }

            PlayStatusCommunicator.PlayStatusUpdated += UpdatePlayButton;
            UpdatePlayButton(PlayStatusCommunicator.CurrentPlayStatus);
        }

        private void OnDestroy()
        {
            PlayStatusCommunicator.PlayStatusUpdated -= UpdatePlayButton;
            TournamentCommunicator.TournamentUpdated -= UpdateTournamentView;
            if (PlayPadCommunicator.Instance?.LobbyCommunicator is { } lobby)
                lobby.OnLobbyInfoUpdated -= OnHostLobbyInfoUpdated;
        }

        // Like the server-client template's LaunchModeResolver: play the page's match once, as soon as it's ready.
        private void PlayHostMatchWhenReady()
        {
            var lobby = PlayPadCommunicator.Instance!.LobbyCommunicator!;
            if (lobby.Lobby.IsMatchReady)
                PlayHostMatch();
            else
                lobby.OnLobbyInfoUpdated += OnHostLobbyInfoUpdated;
        }

        private void OnHostLobbyInfoUpdated(LobbyInfo info)
        {
            if (info.IsMatchReady)
                PlayHostMatch();
        }

        private void PlayHostMatch()
        {
            if (hostMatchRequested)
                return;
            hostMatchRequested = true;
            PlayPadCommunicator.Instance!.LobbyCommunicator!.OnLobbyInfoUpdated -= OnHostLobbyInfoUpdated;
            PlayGameAsync(PlayWebLobbyMatch()).Forget();
        }

        private void UpdateTournamentView(TournamentInfo info)
        {
            //Check and select new current play button view & enable / disable all play buttons depending on tournament type
            for (var i = 0; i < playButtonViews.Length; i++)
            {
                playButtonViews[i].OnTournamentViewUpdated(info);

                if (playButtonViews[i].ShouldUseThisPlayButtonView(info))
                    currentPlayButtonView = playButtonViews[i];

                if (HostMatchmakes)
                    playButtonViews[i].gameObject.SetActive(false);
            }
        }

        private void UpdatePlayButton(PlayStatusInfo info)
        {
            currentPlayButtonView.UpdatePlayButton(info);
        }

#if UNITY_EDITOR || !UNITY_WEBGL || ELYMPICS_PLAYPAD_KEEP_LOBBY
        // The web page blocks play when it finds no tournament, so without the Tournament feature the sample matchmakes by itself.
        private static bool PlaysWithoutTournament => PlayPadCommunicator.Instance!.SessionManager.CurrentSession?.Features.HasTournament() != true;

        private void PlayTournament() => PlayGameAsync(PlaysWithoutTournament
            ? StartQuickMatch()
            : PlayStatusCommunicator.PlayGame(new PlayGameConfig { QueueName = playQueue })).Forget();

        private void PlayTraining() => PlayGameAsync(StartQuickMatch()).Forget();

        private UniTask<IRoom> StartQuickMatch() => PlayPadCommunicator.Instance!.Lobby.RoomsManager.StartQuickMatch(playQueue);
#else
        private static bool PlaysWithoutTournament => false;

        // Without the lobby, the web page matchmakes and both buttons play the match it found.
        private void PlayTournament() => PlayGameAsync(PlayWebLobbyMatch()).Forget();

        private void PlayTraining() => PlayGameAsync(PlayWebLobbyMatch()).Forget();
#endif

        private static UniTask PlayWebLobbyMatch()
        {
            PlayPadCommunicator.Instance!.LobbyCommunicator!.PlayMatch();
            return UniTask.CompletedTask;
        }

        private async UniTask PlayGameAsync(UniTask matchTask)
        {
            Assert.IsNotNull(MatchConnectingMask.Instance);
            MatchConnectingMask.Instance.ShowOrUpdate(ConnectingSubText);

            try
            {
                await matchTask;
            }
            catch (Exception e)
            {
                if (PlayStatusCommunicator.CurrentPlayStatus.PlayStatus == PlayStatus.UserActionRequired)
                {
                    Debug.Log($"[PlayGameAsync]: {e.Message}");
                }
                else
                {
                    Debug.LogError($"[PlayGameAsync]: {e}");
                    errorScreen.Show($"Connecting to match failed. {e.Message}");
                }

                MatchConnectingMask.Instance.Hide();
            }
        }
    }
}
