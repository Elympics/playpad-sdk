#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using ElympicsPlayPad.ExternalCommunicators;
using ElympicsPlayPad.ExternalCommunicators.Leaderboard;
using ElympicsPlayPad.ExternalCommunicators.Tournament;
using ElympicsPlayPad.Leaderboard;
using ElympicsPlayPad.Protocol.RequestResponse.Leaderboard;
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
        public TournamentPlayState PlayState { get; private set; }

        [PublicAPI]
        public bool IsTournamentAvailable => _tournamentInfo is not null;

        [PublicAPI]
        public TournamentInfo TournamentInfo => _tournamentInfo ?? throw new TournamentException("No tournament is available");

        private TournamentInfo? _tournamentInfo;
        private IRoomsManager _roomsManager = null!;
        private IExternalTournamentCommunicator _externalTournamentCommunicator = null!;
        private IExternalLeaderboardCommunicator _externalLeaderboardCommunicator = null!;
        private CancellationTokenSource? _timerCts;
        private readonly Dictionary<string, string> _tournamentCustomMatchmakingData = new(1);

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                if (PlayPadCommunicator.Instance!.LeaderboardCommunicator == null)
                    throw new ElympicsException($"{nameof(ElympicsTournament)} requires {nameof(PlayPadCommunicator.Instance.LeaderboardCommunicator)}");
                _externalLeaderboardCommunicator = PlayPadCommunicator.Instance!.LeaderboardCommunicator;

                if (PlayPadCommunicator.Instance.TournamentCommunicator == null)
                    throw new ElympicsException($"{nameof(ElympicsTournament)} requires {nameof(PlayPadCommunicator.Instance.TournamentCommunicator)}");
                _externalTournamentCommunicator = PlayPadCommunicator.Instance.TournamentCommunicator;

                _roomsManager = ElympicsLobbyClient.Instance!.RoomsManager;
                _externalTournamentCommunicator.TournamentUpdated += OnTournamentUpdated;

                DontDestroyOnLoad(gameObject);
            }
            else
            {
                ElympicsLogger.LogError($"{nameof(ElympicsTournament)} singleton already exist. Destroying duplicate on {gameObject.name}");
                Destroy(gameObject);
            }
        }

        [PublicAPI]
        public async UniTask Initialize(TournamentInfo tournament)
        {
            Debug.Log($"Initialize {nameof(ElympicsTournament)}.");
            _tournamentInfo = tournament;
            _tournamentCustomMatchmakingData.Clear();
            _tournamentCustomMatchmakingData.Add(TournamentConst.TournamentIdKey, tournament.Id);
            await SetPlayState();
            StartTimer();
            SendEventsOnInitialization();
            Debug.Log($"Tournament Initialized: {tournament.ToString()}");
            await UniTask.CompletedTask;
        }

        [PublicAPI]
        public async UniTask<LeaderboardStatus> FetchTournamentLeaderboard(int? pageNumber, int? pageSize)
        {
            ThrowIfNoTournament();
            return await _externalLeaderboardCommunicator.FetchLeaderboard(_tournamentInfo.Value.Id, null, new LeaderboardTimeScope(_tournamentInfo.Value.StartDate, _tournamentInfo.Value.EndDate), pageNumber ?? 1, pageSize ?? _tournamentInfo.Value.LeaderboardCapacity, LeaderboardRequestType.Regular);
        }

        [PublicAPI]
        public async UniTask<float> FetchUserHighScore()
        {
            ThrowIfNoTournament();
            var result = await _externalLeaderboardCommunicator.FetchUserHighScore(_tournamentInfo.Value.Id, null, new LeaderboardTimeScope(_tournamentInfo.Value.StartDate, _tournamentInfo.Value.EndDate), 1, 1);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (result.Score != -1.0f)
                return result.Score;
            throw new TournamentException("High-score for user has not been found.");
        }

        /// <summary>
        /// Determines whether the user is eligible to participate in a tournament.
        /// Check the documentation for details on error codes.
        /// A <c>StatusCode</c> of 0 indicates that the user can play in the tournament.
        /// </summary>
        /// <returns>
        /// A <see cref="ParticipationInfo"/> object containing the status code and error message, if applicable.
        /// </returns>
        /// <remarks>
        /// This method communicates with an PlayPad to verify the user's eligibility.
        /// </remarks>
        [PublicAPI]
        public async UniTask<ParticipationInfo> CanParticipate()
        {
            var result = await _externalTournamentCommunicator.CanPlayTournament();
            return new ParticipationInfo
            {
                StatusCode = result.statusCode,
                Message = result.message
            };
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
            var info = await CanParticipate();
            if (info.StatusCode != 0)
                throw new TournamentException($"Can't start game. ErrorCode: {info.StatusCode} Reason: {info.Message}");

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
            Initialize(newTournamentInfo).ContinueWith(() =>
            {
                _tournamentInfo = newTournamentInfo;
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
                RequestTimer(_tournamentInfo.Value.StartDate, OnTournamentStarted, _timerCts.Token).Forget();
                Debug.Log("Timer: Tournament Started.");
            }
            else if (IsTournamentOngoing())
            {
                RequestTimer(_tournamentInfo.Value.EndDate, OnTournamentEnded, _timerCts.Token).Forget();
            }
            return;

            async UniTask OnTournamentStarted()
            {
                Debug.Log("Timer: Tournament Finished.");
                await SetPlayState();
                StartTimer();
                TournamentStarted?.Invoke();
            }

            async UniTask OnTournamentEnded()
            {
                await SetPlayState();
                TournamentFinished?.Invoke();
            }
        }
        private bool IsTournamentUpcoming() => DateTimeOffset.Now < _tournamentInfo?.StartDate;
        private bool IsTournamentFinished() => DateTimeOffset.Now > _tournamentInfo?.EndDate;
        private bool IsTournamentOngoing() => _tournamentInfo?.StartDate <= DateTimeOffset.Now && DateTimeOffset.Now < _tournamentInfo?.EndDate;
        private async UniTask SetPlayState()
        {
            var info = await CanParticipate();
            PlayState = info.StatusCode == 0 ? TournamentPlayState.Playable : TournamentPlayState.NonPlayable;
            Debug.Log($"Tournament PlayState is {PlayState}.");
        }
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
        internal async UniTask Initialize(TournamentInfo tournament, IRoomsManager roomsManager)
        {
            _tournamentInfo = tournament;
            _tournamentCustomMatchmakingData.Clear();
            _tournamentCustomMatchmakingData.Add(TournamentConst.TournamentIdKey, tournament.Id.ToString());
            _roomsManager = roomsManager;
            SetPlayState();
            StartTimer();
            await UniTask.CompletedTask;
        }

        private void ThrowIfNoTournament()
        {
            if (_tournamentInfo == null)
                throw new TournamentException("Tournament is not available.");
        }
    }
}
