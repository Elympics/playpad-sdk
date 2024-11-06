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
        bool IsTournamentAvailable { get; }
        TournamentInfo? TournamentInfo { get; }
        UniTask Initialize();
        UniTask<IRoom> FindTournamentMatch(string queueName);
    }
}
