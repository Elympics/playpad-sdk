#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using ElympicsPlayPad.ExternalCommunicators;
using ElympicsPlayPad.ExternalCommunicators.GameStatus;
using ElympicsPlayPad.ExternalCommunicators.GameStatus.Models;
using ElympicsPlayPad.ExternalCommunicators.Tournament;
using ElympicsPlayPad.Tournament.Data;
using ElympicsPlayPad.Tournament.Utility;
using ElympicsPlayPad.Utility;
using JetBrains.Annotations;
using UnityEngine;

namespace ElympicsPlayPad.Tournament
{
    [DefaultExecutionOrder(ElympicsLobbyExecutionOrders.ElympicsTournament)]
    public class ElympicsTournament : MonoBehaviour, IElympicsTournament
    {
        [PublicAPI]
        public event Action? TournamentFinished;

        [PublicAPI]
        public event Action? TournamentStarted;

        [PublicAPI]
        public event Action? TournamentUpdated;

        [PublicAPI]
        public static ElympicsTournament Instance = null!;

        [PublicAPI]
        public bool IsTournamentAvailable => TournamentInfo is not null;

        [PublicAPI]
        public PlayStatusInfo? TournamentStatusPlayState => _externalGameStatusCommunicator.CurrentPlayStatus;

        [PublicAPI]
        public TournamentInfo? TournamentInfo => _externalTournamentCommunicator.CurrentTournament;

        private IRoomsManager _roomsManager = null!;
        private IExternalTournamentCommunicator _externalTournamentCommunicator = null!;
        private IExternalGameStatusCommunicator _externalGameStatusCommunicator = null!;
        private CancellationTokenSource? _timerCts;
        private readonly Dictionary<string, string> _tournamentCustomMatchmakingData = new(1);

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                if (PlayPadCommunicator.Instance.TournamentCommunicator == null)
                    throw new ElympicsException($"{nameof(ElympicsTournament)} requires {nameof(PlayPadCommunicator.Instance.TournamentCommunicator)}");
                _externalTournamentCommunicator = PlayPadCommunicator.Instance.TournamentCommunicator;

                if (PlayPadCommunicator.Instance.GameStatusCommunicator == null)
                    throw new ElympicsException($"{nameof(ElympicsTournament)} requires {nameof(PlayPadCommunicator.Instance.GameStatusCommunicator)}");
                _externalGameStatusCommunicator = PlayPadCommunicator.Instance.GameStatusCommunicator;

                _roomsManager = ElympicsLobbyClient.Instance!.RoomsManager;
                _externalTournamentCommunicator.TournamentUpdated += OnTournamentUpdated;

                DontDestroyOnLoad(gameObject);
            }
            else
            {
                ElympicsLogger.LogWarning($"{nameof(ElympicsTournament)} singleton already exist. Destroying duplicate on {gameObject.name}");
                Destroy(gameObject);
            }
        }

        [PublicAPI]
        public async UniTask Initialize()
        {
            Debug.Log($"Initialize {nameof(ElympicsTournament)}.");
            _tournamentCustomMatchmakingData.Clear();
            _tournamentCustomMatchmakingData.Add(TournamentConst.TournamentIdKey, TournamentInfo!.Value.Id);
            StartTimer();
            SendEventsOnInitialization();
            Debug.Log($"Tournament Initialized: {TournamentInfo!.Value.Id}");
            await UniTask.CompletedTask;
        }

        /// <summary>
        /// Run matchmaker to find tournament game.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        /// <exception cref="TournamentException">Will be thrown if the user is unable to play due to not satisfying the required conditions.</exception>
        [PublicAPI]
        public async UniTask<IRoom> FindTournamentMatch(string queueName)
        {
            var info = await _externalGameStatusCommunicator.CanPlayGame(true);
            if (info.PlayStatus != 0)
                throw new TournamentException($"Can't start game. ErrorCode: {info.PlayStatus} Reason: {info.LabelInfo}");

            return await _roomsManager.StartQuickMatch(queueName, null, null, null, _tournamentCustomMatchmakingData);
        }

        private void SendEventsOnInitialization()
        {
            if (IsTournamentFinished())
                TournamentFinished?.Invoke();
            else if (IsTournamentOngoing())
                TournamentStarted?.Invoke();
        }

        private void OnTournamentUpdated(TournamentInfo newTournamentInfo)
        {
            Debug.Log($"Update tournament using new info: {newTournamentInfo.ToString()}.");
            Initialize().ContinueWith(() =>
            {
                TournamentUpdated?.Invoke();
            }).Forget();
        }

        private void StartTimer()
        {
            if (_timerCts is not null)
            {
                _timerCts.Cancel();
                _timerCts.Dispose();
            }

            _timerCts = new CancellationTokenSource();

            ThrowIfNoTournament();

            if (IsTournamentUpcoming())
            {
                RequestTimer(TournamentInfo!.Value.StartDate, OnTournamentStarted, _timerCts.Token).Forget();
                Debug.Log("Timer: Tournament Started.");
            }
            else if (IsTournamentOngoing())
            {
                RequestTimer(TournamentInfo!.Value.EndDate, OnTournamentEnded, _timerCts.Token).Forget();
            }
            return;

            async UniTask OnTournamentStarted()
            {
                Debug.Log("Timer: Tournament Finished.");
                StartTimer();
                TournamentStarted?.Invoke();
            }

            async UniTask OnTournamentEnded()
            {
                TournamentFinished?.Invoke();
            }
        }
        private bool IsTournamentUpcoming() => DateTimeOffset.Now < TournamentInfo?.StartDate;
        private bool IsTournamentFinished() => DateTimeOffset.Now > TournamentInfo?.EndDate;
        private bool IsTournamentOngoing() => TournamentInfo?.StartDate <= DateTimeOffset.Now && DateTimeOffset.Now < TournamentInfo?.EndDate;
        private async UniTask RequestTimer(DateTimeOffset date, Func<UniTask> callback, CancellationToken ct = default)
        {
            Debug.Log($"Request timer for {date:O}");
            var isCanceled = await UniTask.WaitUntil(() => DateTimeOffset.Now > date, PlayerLoopTiming.Update, ct).SuppressCancellationThrow();
            if (isCanceled)
            {
                Debug.Log("Timer has been canceled.");
                return;
            }
            callback.Invoke().Forget();
        }
        private void OnDestroy()
        {
            _externalTournamentCommunicator.TournamentUpdated -= OnTournamentUpdated;
            _timerCts?.Cancel();
        }
        internal async UniTask Initialize(IRoomsManager roomsManager)
        {
            _tournamentCustomMatchmakingData.Clear();
            _tournamentCustomMatchmakingData.Add(TournamentConst.TournamentIdKey, TournamentInfo!.Value.Id);
            _roomsManager = roomsManager;
            StartTimer();
            await UniTask.CompletedTask;
        }

        private void ThrowIfNoTournament()
        {
            if (TournamentInfo == null)
                throw new TournamentException("Tournament is not available.");
        }
    }
}
