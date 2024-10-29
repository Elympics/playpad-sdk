using System;
using Cysharp.Threading.Tasks;
using Elympics;
using ElympicsPlayPad.Leaderboard;
using ElympicsPlayPad.Tournament.Data;

namespace ElympicsPlayPad.Tournament
{
    public interface IElympicsTournament
    {
        event Action TournamentFinished;
        event Action TournamentStarted;
        TournamentPlayState PlayState { get; }
        bool IsTournamentAvailable { get; }
        TournamentInfo TournamentInfo { get; }
        UniTask Initialize(TournamentInfo tournament);
        UniTask<IRoom> FindTournamentMatch(string queueName);
        UniTask<float> FetchUserHighScore();
        UniTask<LeaderboardStatus> FetchTournamentLeaderboard(int? pageNumber, int? pageSize);
    }
}
